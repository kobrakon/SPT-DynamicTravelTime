"use strict";

function removeByteOrderMark(str){
    return str.replace(/^\ufeff/g,"")
}

function getFactory() {
    var fs = require('fs');
    var file = fs.readFileSync('./user/mods/r1ft-PTTDynamicTimeCycle/cfg/persistance.json', 'utf8');
    var json = JSON.parse(removeByteOrderMark(file));
    var hideout = json.hideout;
    var hour = json.currentHour;
    if (!hideout) {
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

class PTTDynamicTravelTime {
    static onLoadMod() {
        if (!globalThis.PathToTarkovAPI) {
            Logger.error(`=> ${this.modName}: PathToTarkovAPI not found, are you sure a version of PathToTarkov >= 2.5.0 is installed ?`);
            return;
        }

        PathToTarkovAPI.onStart(() => {
            getFactory();
        })

        HttpRouter.onStaticRoute["/client/locations"]["ZZZ-ZZZ-MUSTBELAST-PTTDynamicTravelTime"] = PTTDynamicTravelTime.RouteLocations;
    }

    static RouteLocations(url, info, sessionID) {
        getFactory();
        return HttpResponse.getBody(LocationController.generateAll())
    }
}

module.exports = PTTDynamicTravelTime;