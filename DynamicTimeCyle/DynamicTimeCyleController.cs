using Comfort.Common;
using EFT;
using EFT.UI;
using EFT.Weather;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace r1ft.DynamicTimeCyle
{
    public class DynamicTimeCyleController : MonoBehaviour
    {
        private static GameWorld gameWorld;

        private static Type gameDateTime;
        private static MethodInfo CalculateTime;
        private static MethodInfo ResetTime;

        private static string _offraidPos = "";

        private static double _cacheTimeHour = 99;
        private static double _cacheTimeMin = 99;

        private static bool _init = false;

        private static readonly string serverlog = Path.Combine(Application.dataPath, "..\\user\\logs\\server.log");
        private static readonly string pttconfig = Path.Combine(Application.dataPath, "..\\user\\mods\\ZTrap-PathToTarkov\\config\\travel.json");

        private static List<PTTConfig.Locations> _pttConfig = null;

        private bool IsTargetMethod(MethodInfo mi)
        {
            var parameters = mi.GetParameters();
            return parameters.Length == 1
                && parameters[0].Name == "gameDateTime";
        }

        public void Update()
        {
            if (_pttConfig == null)
                _pttConfig = Reader.ReadPTTConfig(pttconfig);

            gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null)
            {
                if (_init)
                {
                    var offraidPos = Reader.GetOffRaidPos(serverlog);
                    DEBUGMsg($"offraid pos: {offraidPos}");
                    if (offraidPos != _offraidPos)
                        _offraidPos = offraidPos;
                    _init = !_init;
                }

                return;
            }

            if (GameObject.Find("Weather") == null)
                return;

            if (gameWorld.AllPlayers == null)
                return;

            if (gameWorld.AllPlayers.Count == 0)
                return;

            if (gameWorld.AllPlayers[0] is HideoutPlayer)
                return;

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
            if (_cacheTimeHour == 99 && _cacheTimeMin == 99)
            {
                _cacheTimeHour = currentDateTime.Hour;
                _cacheTimeMin = currentDateTime.Minute;
                DEBUGMsg($"Cache Time was set to: {_cacheTimeHour}:{_cacheTimeMin}");
            }

            if (!_init)
            {
                var hourOffset = _cacheTimeHour;
                var minOffset = _cacheTimeMin;
                if (_offraidPos != "")
                {
                    var config = _pttConfig.ToArray();
                    foreach (var location in config)
                    {
                        if (location.OffraidPosition != _offraidPos)
                            continue;

                        minOffset += location.TravelTime;
                        break;
                    }

                    if (minOffset > 60)
                    {
                        hourOffset += Mathf.RoundToInt((int)minOffset / 60);
                        minOffset -= Mathf.RoundToInt((int)minOffset / 60) * 60;
                    }
                }
                var modifiedDateTime = currentDateTime.AddHours(hourOffset - currentDateTime.Hour);
                modifiedDateTime = modifiedDateTime.AddMinutes(minOffset - currentDateTime.Minute);
                ResetTime.Invoke(gameWorld.GameDateTime, new object[] { modifiedDateTime });
                DEBUGMsg($"Time was set to: {modifiedDateTime:HH:mm}");
                _init = !_init;
                return;
            }

            _cacheTimeHour = currentDateTime.Hour;
            _cacheTimeMin = currentDateTime.Minute;
            return;
        }

        private static void DEBUGMsg(string msg)
        {
#if debug
                PreloaderUI.Instance.Console.AddLog(msg, "DEBUG");
#endif
            return;
        }
    }
}
