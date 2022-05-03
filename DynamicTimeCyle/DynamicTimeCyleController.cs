using Comfort.Common;
using EFT;
using EFT.UI;
using System;
using System.Collections.Generic;
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
        private static double _nightTimeAccelTime = 0;

        private static bool _init = true;
        private static bool _pttNotInit = false;
        private static bool _firstStart = true;
        private static bool _hideout = false;
        private static bool _pttEnabled = false;
        private static bool _nightTimeAccel = false;

        private static List<Config.Locations> _dtcConfig = null;
        private static Config.PTTConfig _pttConfig = null;

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

            if (_firstStart)
            {
                if (!PreloaderUI.Instance.MenuTaskBar.isActiveAndEnabled)
                    return;

                _dtcConfig = Config.GetDTCConfig(out _pttEnabled, out _nightTimeAccel, out _nightTimeAccelTime);

                if (_pttEnabled)
                {
                    _pttConfig = Config.GetPTTConfig();
                }

                _firstStart = false;
            }

            gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null)
            {
                if (_init)
                {
                    _pttNotInit = !Network.SetOffRaidPosition(_pttEnabled, _pttConfig, _cacheTimeHour, _cacheTimeMin, _nightTimeAccel, _nightTimeAccelTime, out _offraidPos, out _cacheTimeHour, out _cacheTimeMin, out _hideout);
                    if (_pttNotInit)
                        return;

                    Notifier.DrawCurrentTime(_pttEnabled, _hideout, _cacheTimeHour, _cacheTimeMin);
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
                        DynamicTime.ReturnMapTime(_cacheTimeHour, _cacheTimeMin, _pttEnabled ? _offraidPos : gameWorld.AllPlayers[0].Location, _dtcConfig.ToArray(), out _cacheTimeHour, out _cacheTimeMin);         
                    else
                    {
                        if (gameWorld.AllPlayers[0].Location == null)
                            return;

                        DynamicTime.ReturnFactoryTime(gameWorld.AllPlayers[0].Location, out _cacheTimeHour, out _cacheTimeMin);
                    }
                    _init = true;
                    return;
                }

                if (!_init && _hideout)
                {
                    if (gameWorld.AllPlayers[0].Location == null)
                        return;

                    DynamicTime.ReturnFactoryTime(gameWorld.AllPlayers[0].Location, out _cacheTimeHour, out _cacheTimeMin);
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
                DynamicTime.ReturnMapTime(_cacheTimeHour, _cacheTimeMin, _pttEnabled ? _offraidPos : gameWorld.AllPlayers[0].Location, _dtcConfig.ToArray(), out var hourOffset, out var minOffset);
                var modifiedDateTime = currentDateTime.AddHours(hourOffset - currentDateTime.Hour);
                modifiedDateTime = modifiedDateTime.AddMinutes(minOffset - currentDateTime.Minute);
                ResetTime.Invoke(gameWorld.GameDateTime, new object[] { modifiedDateTime });
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
                _init = true;
                return;
            }

            _cacheTimeHour = currentDateTime.Hour;
            _cacheTimeMin = currentDateTime.Minute;

            return;
        }
    }
}
