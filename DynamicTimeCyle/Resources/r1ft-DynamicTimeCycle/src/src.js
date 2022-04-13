"use strict";

const config = require("../cfg/config.json")

function removeQuotesMark(str) {
    return str.replace(/^\"/g, "")
}

function getFactory(sessionID) {
    var profile = SaveServer.profiles[sessionID];
    var hideout = profile.DynamicTimeCycle.hideout;
    var hour = removeQuotesMark(profile.DynamicTimeCycle.hour);
    if (!DatabaseServer.tables.locations.factory4_night.base.Locked || !DatabaseServer.tables.locations.factory4_day.base.Locked) {
        if (hideout == "false") {
            if (hour > 5 && hour < 19) {
                Logger.info("=> Dynamic Time Cycle : Factory Night Locked");
                DatabaseServer.tables.locations.factory4_night.base.Locked = true;
                DatabaseServer.tables.locations.factory4_day.base.Locked = false;
            }
            else {
                Logger.info("=> Dynamic Time Cycle : Factory Day Locked");
                DatabaseServer.tables.locations.factory4_night.base.Locked = false;
                DatabaseServer.tables.locations.factory4_day.base.Locked = true;
            }
        }
        else {
            Logger.info("=> Dynamic Time Cycle : Factory Unocked");
            DatabaseServer.tables.locations.factory4_night.base.Locked = false;
            DatabaseServer.tables.locations.factory4_day.base.Locked = false;
        }
    }
}

class DynamicTimeCycle {
    static onLoadMod() {
        var pttEnabled = config.PTTEnabled
        if (pttEnabled) {
            if (!globalThis.PathToTarkovAPI) {
                Logger.error(`=> Dynamic Time Cycle: PathToTarkovAPI not found Disabling PTT Options`);
                pttEnabled = false;
            }
        }

        if (pttEnabled) {
            HttpRouter.onStaticRoute["/dynamictimecycle/offraidPosition"] = {
                config: DynamicTimeCycle.onRequestPosition.bind(this)
            };
        }

        HttpRouter.onStaticRoute["/dynamictimecycle/ptt"] = {
            config: DynamicTimeCycle.onRequestptt.bind(this)
        };

        HttpRouter.onStaticRoute["/dynamictimecycle/config"] = {
            config: DynamicTimeCycle.onRequestConfig.bind(this)
        };

        HttpRouter.onDynamicRoute["/dynamictimecycle/post/"] = {
            postconfig: DynamicTimeCycle.onRequesPostConfig.bind(this)
        };
    }

    static onRequestConfig(url, info, sessionID) {
        var profile = SaveServer.profiles[sessionID];
        if (profile.DynamicTimeCycle == null) {
            Logger.info("=> Dynamic Time Cycle : Creating Profile");
            profile.DynamicTimeCycle = {};
            profile.DynamicTimeCycle.hour = 99;
            profile.DynamicTimeCycle.min = 99;
            profile.DynamicTimeCycle.hideout = true;
        }

        Logger.info("=> Dynamic Time Cycle : Returning Config");
        return HttpResponse.noBody(SaveServer.profiles[sessionID].DynamicTimeCycle);
    }

    static onRequestptt(url, info, sessionID) {
        var profile = SaveServer.profiles[sessionID];
        if (profile.PathToTarkov == null) {
            Logger.info("=> Dynamic Time Cycle : PTT Not Available");
            return HttpResponse.noBody(false);
        }

        Logger.info("=> Dynamic Time Cycle : PTT Available");
        return HttpResponse.noBody(true);
    }

    static onRequestPosition(url, info, sessionID) {
        var profile = SaveServer.profiles[sessionID];
        if (profile.PathToTarkov == null) {
            profile.PathToTarkov = {};
            profile.PathToTarkov.mainStashId = "";
            profile.PathToTarkov.offraidPosition = "null";
        }

        Logger.info("=> Dynamic Time Cycle : Returning PTT Config");
        return HttpResponse.noBody(SaveServer.profiles[sessionID].PathToTarkov);
    }

    static onRequesPostConfig(url, info, sessionID) {
        var profile = SaveServer.profiles[sessionID];
        const splittedUrl = url.split("/");

        profile.DynamicTimeCycle.hour = splittedUrl[splittedUrl.length - 3].toLowerCase();
        profile.DynamicTimeCycle.min = splittedUrl[splittedUrl.length - 2].toLowerCase();
        profile.DynamicTimeCycle.hideout = splittedUrl[splittedUrl.length - 1].toLowerCase();

        getFactory(sessionID);

        Logger.info("=> Dynamic Time Cycle : Updating Config");
        return HttpResponse.noBody(SaveServer.profiles[sessionID].DynamicTimeCycle);
    }
}

module.exports = DynamicTimeCycle;