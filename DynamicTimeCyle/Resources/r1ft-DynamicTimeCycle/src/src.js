"use strict";

function removeByteOrderMark(str) {
    return str.replace(/^\ufeff/g, "")
}

function removeQuotesMark(str) {
    return str.replace(/^\"/g, "")
}

function getFactory(sessionID) {
    var profile = SaveServer.profiles[sessionID];
    var hideout = profile.DynamicTimeCycle.hideout;
    var hour = removeQuotesMark(hideout);

    if (hideout == "false") {
        if (hour > 5 && hour < 19) {
            Logger.info("=> PTT Dynamic Time Cycle : Factory Night Locked");
            DatabaseServer.tables.locations.factory4_night.base.Locked = true;
            DatabaseServer.tables.locations.factory4_day.base.Locked = false;
        }
        else {
            Logger.info("=> PTT Dynamic Time Cycle : Factory Day Locked");
            DatabaseServer.tables.locations.factory4_night.base.Locked = false;
            DatabaseServer.tables.locations.factory4_day.base.Locked = true;
        }
    }
    else {
        Logger.info("=> PTT Dynamic Time Cycle : Factory Unocked");
        DatabaseServer.tables.locations.factory4_night.base.Locked = false;
        DatabaseServer.tables.locations.factory4_day.base.Locked = false;
    }
}

class DynamicTimeCycle {
    static onLoadMod() {
        if (!globalThis.PathToTarkovAPI) {
            Logger.error(`=> ${this.modName}: PathToTarkovAPI not found, are you sure a version of PathToTarkov >= 2.5.0 is installed ?`);
            return;
        }

        HttpRouter.onStaticRoute["/dynamictimecycle/offraidPosition"] = {
            config: DynamicTimeCycle.onRequestPosition.bind(this)
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
            profile.DynamicTimeCycle = {};
            profile.DynamicTimeCycle.hour = 99;
            profile.DynamicTimeCycle.min = 99;
            profile.DynamicTimeCycle.hideout = true;
        }

        return HttpResponse.noBody(SaveServer.profiles[sessionID].DynamicTimeCycle);
    }

    static onRequestPosition(url, info, sessionID) {
        var profile = SaveServer.profiles[sessionID];
        if (profile.PathToTarkov == null) {
            profile.PathToTarkov = {};
            profile.PathToTarkov.mainStashId = "";
            profile.PathToTarkov.offraidPosition = "null";
        }

        return HttpResponse.noBody(SaveServer.profiles[sessionID].PathToTarkov);
    }

    static onRequesPostConfig(url, info, sessionID) {
        var profile = SaveServer.profiles[sessionID];
        const splittedUrl = url.split("/");

        profile.DynamicTimeCycle.hour = splittedUrl[splittedUrl.length - 3].toLowerCase();
        profile.DynamicTimeCycle.min = splittedUrl[splittedUrl.length - 2].toLowerCase();
        profile.DynamicTimeCycle.hideout = splittedUrl[splittedUrl.length - 1].toLowerCase();

        getFactory(sessionID);

        return HttpResponse.noBody(SaveServer.profiles[sessionID].DynamicTimeCycle);
    }
}

module.exports = DynamicTimeCycle;