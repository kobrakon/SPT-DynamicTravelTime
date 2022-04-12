using Comfort.Common;
using EFT;
using EFT.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace r1ft.DynamicTimeCyle
{
    public class DynamicTimeCycleController : MonoBehaviour
    {
        private static GameWorld gameWorld;

        private static Type gameDateTime;
        private static MethodInfo CalculateTime;
        private static MethodInfo ResetTime;

        private static string _offraidPos = "";

        private static double _cacheTimeHour = 99;
        private static double _cacheTimeMin = 99;

        private static float _waitTime = 30;
        private static float _cacheWait = 0;
        private static float _startWait = 0;

        private static bool _init = true;
        private static bool _hideout = false;
        private static bool _pttNotInit = false;
        private static bool _firstStart = true;

        private static readonly string pttconfig = Path.Combine(Application.dataPath, "..\\user\\mods\\r1ft-PTTDynamicTimeCycle\\cfg\\traveltime.json");
        private static readonly string pttconfigmain = Path.Combine(Application.dataPath, "..\\user\\mods\\ZTrap-PathToTarkov\\config\\config.json");

        private static List<PTTConfig.Locations> _pttConfig = null;
        private static PTTConfig.MainConfig _pttConfigMain = null;

        private bool IsTargetMethod(MethodInfo mi)
        {
            var parameters = mi.GetParameters();
            return parameters.Length == 1
                && parameters[0].Name == "gameDateTime";
        }

        public void Update()
        {
            if (!PreloaderUI.Instantiated)
                return;

            if (PreloaderUI.Instance == null)
                return;

            if (!PreloaderUI.Instance.CanShowErrorScreen)
                return;

            if (_pttConfigMain == null)
                _pttConfigMain = Reader.GetConfig(pttconfigmain);


            if (_pttConfig == null)
            {
                _pttConfig = Reader.GetConfig(_pttConfigMain, pttconfig);
                return;
            }

            if (_pttNotInit)
            {
                if (_cacheWait == 0)
                    _cacheWait = Time.time + _waitTime;                

                if (Time.time > _cacheWait)
                    _pttNotInit = false;

                return;
            }

            if (_firstStart)
            {
                if (_startWait == 0)
                    _startWait = Time.time + _waitTime;

                if (Time.time > _startWait)
                {
                    _firstStart = false;
                }
            }

            gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null)
            {
                if (_init)
                {
                    _pttNotInit = !SetOffRaidPosition(_pttConfigMain);
                    if (_pttNotInit)
                        return;

                    DrawCurrentTime();
                    _init = false;
                }
                return;
            }

            if (gameWorld.AllPlayers == null)
                return;

            if (gameWorld.AllPlayers.Count == 0)
                return;

            if (gameWorld.AllPlayers[0] is HideoutPlayer)
                return;

            if (GameObject.Find("Weather") == null)
            {
                if (!_init && !_hideout)
                {
                    if (_cacheTimeHour != 99 && _cacheTimeMin != 99)
                    {
                        CalculateTimeOffset(out _cacheTimeHour, out _cacheTimeMin);
                    }
                    else
                    {
                        if (gameWorld.AllPlayers[0].Location == null)
                            return;

                        if (gameWorld.AllPlayers[0].Location == "factory4_day")
                        {
                            _cacheTimeHour = 15;
                            _cacheTimeMin = 28;
                        }

                        if (gameWorld.AllPlayers[0].Location == "factory4_night")
                        {
                            _cacheTimeHour = 3;
                            _cacheTimeMin = 28;
                        }
                    }
#if DEBUG
                    DEBUGMsg($"offraid pos: {_offraidPos}");
                    DEBUGMsg($"Time was set to: {_cacheTimeHour}:{_cacheTimeMin}");
                    DEBUGMsg("FACTORY OR LABS");
#endif
                    _init = true;
                    return;
                }

                if (!_init && _hideout)
                {
                    if (gameWorld.AllPlayers[0].Location == null)
                        return;

                    if (gameWorld.AllPlayers[0].Location == "factory4_day")
                    {
                        _cacheTimeHour = 15;
                        _cacheTimeMin = 28;
                    }

                    if (gameWorld.AllPlayers[0].Location == "factory4_night")
                    {
                        _cacheTimeHour = 3;
                        _cacheTimeMin = 28;
                    }

#if DEBUG
                    DEBUGMsg($"offraid pos: {_offraidPos}");
                    DEBUGMsg($"Cache Time was set to: {_cacheTimeHour}:{_cacheTimeMin}");
                    DEBUGMsg($"Hideout");
#endif
                    _init = true;
                    return;
                }
                return;
            }

            if (gameWorld.GameDateTime == null)
                return;

            gameDateTime = gameWorld.GameDateTime.GetType();
            if (gameDateTime == null)
                return;

            CalculateTime = gameDateTime.GetMethod("Calculate", BindingFlags.Public | BindingFlags.Instance);
            if (CalculateTime == null)
                return;

            ResetTime = gameDateTime.GetMethods(BindingFlags.Public | BindingFlags.Instance).Single(IsTargetMethod);
            if (ResetTime == null)
                return;

            var currentDateTime = (DateTime)CalculateTime.Invoke(gameWorld.GameDateTime, null);
            if (!_init && !_hideout)
            {
                CalculateTimeOffset(out var hourOffset, out var minOffset);
                var modifiedDateTime = currentDateTime.AddHours(hourOffset - currentDateTime.Hour);
                modifiedDateTime = modifiedDateTime.AddMinutes(minOffset - currentDateTime.Minute);
                ResetTime.Invoke(gameWorld.GameDateTime, new object[] { modifiedDateTime });
#if DEBUG
                DEBUGMsg($"offraid pos: {_offraidPos}");
                DEBUGMsg($"Time was set to: {modifiedDateTime:HH:mm}");
#endif
                currentDateTime = modifiedDateTime;
                _cacheTimeHour = modifiedDateTime.Hour;
                _cacheTimeMin = modifiedDateTime.Minute;
                _init = true;
                return;
            }

            if (!_init && _hideout)
            {
                _cacheTimeHour = currentDateTime.Hour;
                _cacheTimeMin = currentDateTime.Minute;
#if DEBUG
                DEBUGMsg($"offraid pos: {_offraidPos}");
                DEBUGMsg($"Cache Time was set to: {_cacheTimeHour}:{_cacheTimeMin}");
                DEBUGMsg($"Hideout");
#endif
                _init = true;
                return;
            }

            _cacheTimeHour = currentDateTime.Hour;
            _cacheTimeMin = currentDateTime.Minute;

            return;
        }


        private static bool SetOffRaidPosition(PTTConfig.MainConfig main)
        {
            var config = Reader.GetOffRaidPosition(out var err, out var pos);
            if (err)
                return false;

            _offraidPos = pos;

            if (_cacheTimeHour == 99)
            {
                _hideout = config.hideout;
                _cacheTimeHour = config.hour;
                _cacheTimeMin = config.min;
            }

            _hideout = false;
            foreach (var hideoutposition in _pttConfigMain.hideout_main_stash_access_via)
            {
                if (hideoutposition != pos)
                    continue;

                _hideout = true;
                break;
            }

            Writer.WritePersistance(_cacheTimeHour, _cacheTimeMin, _hideout);
#if DEBUG
            DEBUGMsg("SAVED PERSISTANCE");
            DEBUGMsg($"Hour : {_cacheTimeHour}");
            DEBUGMsg($"Min : {_cacheTimeMin}");
            DEBUGMsg($"Position : {_offraidPos}");
            DEBUGMsg($"Hideout : {_hideout}");
#endif
            
            return true;
        }

        private static void DrawCurrentTime()
        {
            if (_hideout || _cacheTimeHour == 99)
                Notifier.DisplayMessageNotification($"Hideout", Color.white);
            else
                Notifier.DisplayMessageNotification($"Current Raid Time: {(_cacheTimeHour < 10 ? "0" : "")}{string.Format("{0:0,0}", _cacheTimeHour)}:{string.Format("{0:0,0}", _cacheTimeMin)}", Color.white);

            return;
        }

        private static void CalculateTimeOffset(out double hour, out double min)
        {
            hour = _cacheTimeHour;
            min = _cacheTimeMin;
            if (_offraidPos != "")
            {
                var config = _pttConfig.ToArray();
                foreach (var location in config)
                {
                    if (location.OffraidPosition != _offraidPos)
                        continue;

                    min += location.TravelTime;
#if DEBUG
                    DEBUGMsg(min.ToString());
#endif
                    break;
                }

                if (min > 60)
                {
                    hour += Mathf.RoundToInt((int)min / 60);
                    min -= Mathf.RoundToInt((int)min / 60) * 60;
                }
            }
        }
#if DEBUG
        private static void DEBUGMsg(string msg)
        {
            PreloaderUI.Instance.Console.AddLog(msg, "[DEBUG]");
            return;
        }
#endif
    }
}
