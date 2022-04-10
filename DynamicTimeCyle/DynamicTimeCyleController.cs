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

        private static bool _init = false;
        private static bool _hideout = false;
        private static bool _persistanceinit = false;

        public static bool _factoryNight = false;

        private static readonly string serverlog = Path.Combine(Application.dataPath, "..\\user\\logs\\server.log");
        private static readonly string pttconfig = Path.Combine(Application.dataPath, "..\\user\\mods\\r1ft-PTTDynamicTimeCycle\\cfg\\traveltime.json");
        private static readonly string pttpersistance = Path.Combine(Application.dataPath, "..\\user\\mods\\r1ft-PTTDynamicTimeCycle\\cfg\\persistance.json");
        private static readonly string pttconfigmain = Path.Combine(Application.dataPath, "..\\user\\mods\\ZTrap-PathToTarkov\\config\\config.json");

        private static List<PTTConfig.Locations> _pttConfig = null;
        private static PTTConfig.MainConfig _pttConfigMain = null;

#if DEBUG
        private bool debug1 = false;
        private bool debug2 = false;
#endif

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

#if DEBUG
            if (debug1)
            {
                var offraidPositions = Reader.GetOffRaidPositions(_pttConfigMain);
                foreach (var position in offraidPositions)
                {
                    DEBUGMsg(position);
                }
                debug1 = !debug1;
            }
#endif

            if (_pttConfig == null)
            {
                _pttConfig = Reader.GetLocation(_pttConfigMain, pttconfig);
                return;
            }

#if DEBUG
            if (debug2)
            {
                foreach (var location in _pttConfig.ToArray())
                {
                    DEBUGMsg($"{location.OffraidPosition} : {location.TravelTime}");
                }
                debug2 = !debug2;
            }
#endif

            gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null)
            {
                SetPersistance();
                _init = false;
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
                        if (gameWorld.LocationId == null)
                            return;

                        if (gameWorld.LocationId == "55f2d3fd4bdc2d5f408b4567")
                        {
                            _cacheTimeHour = 15;
                            _cacheTimeMin = 28;
                        }

                        if (gameWorld.LocationId == "59fc81d786f774390775787e")
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
                    SetOffRaidPosition();
                    return;
                }

                if (!_init && _hideout)
                {
                    if (gameWorld.LocationId == null)
                        return;

                    if (gameWorld.LocationId == "55f2d3fd4bdc2d5f408b4567")
                    {
                        _cacheTimeHour = 15;
                        _cacheTimeMin = 28;
                    }

                    if (gameWorld.LocationId == "59fc81d786f774390775787e")
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
                    SetOffRaidPosition();
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
                SetOffRaidPosition();
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
                SetOffRaidPosition();
                return;
            }

            _cacheTimeHour = currentDateTime.Hour;
            _cacheTimeMin = currentDateTime.Minute;

            return;
        }


        private static void SetOffRaidPosition()
        {
            var offraidPos = Reader.GetOffRaidPos(_pttConfigMain, serverlog, out var found);
            if (_offraidPos == "" || found)
                _offraidPos = offraidPos;

            _hideout = false;
            foreach (var hideoutposition in _pttConfigMain.hideout_main_stash_access_via)
            {
                if (hideoutposition != _offraidPos)
                    continue;

                _hideout = true;
                break;
            }

            Writer.WritePersistance(new PTTConfig.Persistance { currentLocation = _offraidPos, currentHour = _cacheTimeHour, currentMin = _cacheTimeMin, hideout = _hideout }, pttpersistance);
#if DEBUG
            DEBUGMsg("SAVED PERSISTANCE");
            DEBUGMsg($"Hour : {_cacheTimeHour}");
            DEBUGMsg($"Min : {_cacheTimeMin}");
            DEBUGMsg($"Position : {_offraidPos}");
            DEBUGMsg($"Hideout : {_hideout}");
#endif
            Notifier.DisplayMessageNotification($"Current Raid Time: {(_cacheTimeHour < 10 ? "0" : "")}{string.Format("{0:0,0}", _cacheTimeHour)}:{string.Format("{0:0,0}", _cacheTimeMin)}");
            return;
        }

        private static void SetPersistance()
        {
            if (!_persistanceinit)
            {
                if (_cacheTimeHour == 99 && _cacheTimeMin == 99)
                {
                    if (File.Exists(pttpersistance))
                    {
                        var persistance = Reader.ReadPersistance(pttpersistance);
                        if (persistance.currentLocation != "")
                        {
                            _cacheTimeHour = persistance.currentHour;
                            _cacheTimeMin = persistance.currentMin;
                            _offraidPos = persistance.currentLocation;
                            _hideout = persistance.hideout;
#if DEBUG
                            DEBUGMsg("LOADED PERSISTANCE");
                            DEBUGMsg($"Hour : {_cacheTimeHour}");
                            DEBUGMsg($"Min : {_cacheTimeMin}");
                            DEBUGMsg($"Position : {_offraidPos}");
                            DEBUGMsg($"Hideout : {_hideout}");
#endif
                            return;
                        }
                    }
                }
                _persistanceinit = true;
            }
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
