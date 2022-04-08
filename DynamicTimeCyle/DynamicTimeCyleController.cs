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
                _pttConfig = ReadPTTConfig();

            gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null)
            {
                if (_init)
                {
                    var offraidPos = GetOffRaidPos();
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
                var debugstring = "Time was set to: " + modifiedDateTime.ToString("HH:mm");
                DEBUGMsg(debugstring);
                _init = !_init;
                return;
            }

            _cacheTimeHour = currentDateTime.Hour;
            _cacheTimeMin = currentDateTime.Minute;
            return;
        }

        private static void DEBUGMsg(string msg)
        {
            PreloaderUI.Instance.Console.AddLog(msg, "DEBUG");
        }

        private static List<PTTConfig.Locations> ReadPTTConfig()
        {
            System.IO.FileStream f_pttconfig = new System.IO.FileStream(pttconfig, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite);

            System.IO.StreamReader s_pttconfig = new System.IO.StreamReader(f_pttconfig);

            var lst = new List<string>();

            while (!s_pttconfig.EndOfStream)
                lst.Add(s_pttconfig.ReadLine());

            var jsonstring = "";

            foreach (var line in lst.ToArray())
            {
                jsonstring += line.Trim();
            }

            var config = JsonConvert.DeserializeObject<PTTConfig.Locations[]>(jsonstring);
            var list = new List<PTTConfig.Locations>();

            foreach (var option in config)
                list.Add(option);

            return list;
        }

        private static string GetOffRaidPos()
        {
            System.IO.FileStream f_serverlog = new System.IO.FileStream(serverlog, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite);

            System.IO.StreamReader s_serverlog = new System.IO.StreamReader(f_serverlog);

            List<string> lst = new List<string>();

            var offraidpos = "";

            while (!s_serverlog.EndOfStream)
                lst.Add(s_serverlog.ReadLine());

            foreach (var line in lst.ToArray())
            {
                if (!line.Contains("=> PathToTarkov: player offraid position changed to"))
                    continue;

                var splitchar = " ";
                var split = line.Split(splitchar.ToCharArray()[0]);
                offraidpos = split[split.Length - 1].Trim();
                var removechar = "'";
                offraidpos = offraidpos.Replace(removechar.ToCharArray()[0], splitchar.ToCharArray()[0]).Trim();
                DEBUGMsg($"offraid pos: {offraidpos}");

                break;
            }

            return offraidpos;
        }
    }
}
