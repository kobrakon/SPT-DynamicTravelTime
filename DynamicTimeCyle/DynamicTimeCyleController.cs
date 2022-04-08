using Comfort.Common;
using EFT;
using EFT.UI;
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

        private static double _cacheTimeHour = 99;
        private static double _cacheTimeMin = 99;

        private static bool _init = false;

        private static readonly string serverlog = Path.Combine(Application.dataPath, "..\\user\\logs\\server.log");

        private static readonly string pttconfig = Path.Combine(Application.dataPath, "..\\user\\mods\\ZTrap-PathToTarkov\\config\\travel.json");

        private bool IsTargetMethod(MethodInfo mi)
        {
            var parameters = mi.GetParameters();
            return parameters.Length == 1
                && parameters[0].Name == "gameDateTime";
        }

        public void Update()
        {
            gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null)
            {
                if (_init)
                    _init = !_init;

                return;
            }

            if (gameWorld.AllPlayers == null)
                return;

            if (gameWorld.AllPlayers.Count == 0)
                return;

            if (gameWorld.AllPlayers[0] is HideoutPlayer)
                return;


            if (gameWorld.GameDateTime == null)
                return;

            gameDateTime = gameWorld.GameDateTime.GetType();

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
                PreloaderUI.Instance.Console.AddLog($"Cache Time was set to: {_cacheTimeHour}:{_cacheTimeMin}", "");
            }

            if (gameWorld.LocationId == null)
                return;

            MapName.QuestMapNameToIdMap.TryGetValue(gameWorld.LocationId, out var currentMap);

            if (!_init)
            {
                var hourOffset = 0;
                var minOffset = GetOffRaidTime();
                if (minOffset > 60)
                {
                    hourOffset += Mathf.RoundToInt(minOffset / 60);
                    minOffset -= Mathf.RoundToInt(minOffset / 60) * 60;
                }
                var modifiedDateTime = currentDateTime.AddHours(hourOffset - currentDateTime.Hour);
                modifiedDateTime = modifiedDateTime.AddMinutes(minOffset - currentDateTime.Minute);
                ResetTime.Invoke(gameWorld.GameDateTime, new object[] { modifiedDateTime });
                var debugstring = "Time was set to: " + modifiedDateTime.ToString("HH:mm");
                PreloaderUI.Instance.Console.AddLog(debugstring, "");
                _init = !_init;
                return;
            }

            _cacheTimeHour = currentDateTime.Hour;
            _cacheTimeMin = currentDateTime.Minute;
            return;
        }

        public static int GetOffRaidTime()
        {
            System.IO.FileStream f_serverlog = new System.IO.FileStream(serverlog, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite);

            System.IO.StreamReader s_serverlog = new System.IO.StreamReader(f_serverlog);

            List<string> lst = new List<string>();

            var offraidpos = "";

            while (!s_serverlog.EndOfStream)
                lst.Add(s_serverlog.ReadLine());

            s_serverlog.Close();

            f_serverlog.Close();

            foreach (var line in lst.ToArray())
            {
                if (!line.Contains("=> PathToTarkov: player offraid position changed to"))
                    continue;

                var split = line.Split(' ');
                offraidpos = split[split.Length].Trim();
            }



            System.IO.FileStream f_pttconfig = new System.IO.FileStream(pttconfig, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite);

            System.IO.StreamReader s_pttconfig = new System.IO.StreamReader(f_pttconfig);

            lst = new List<string>();

            while (!s_serverlog.EndOfStream)
                lst.Add(s_serverlog.ReadLine());

            s_pttconfig.Close();

            f_pttconfig.Close();

            var traveltime = 0;

            foreach (var line in lst.ToArray())
            {
                var split = line.Split(':');
                var posname = split[0].Replace('"', ' ').Trim();
                if (offraidpos != posname)
                    continue;

                traveltime = int.Parse(split[1].Trim());

                break;

            }

            return traveltime;
        }
    }
}
