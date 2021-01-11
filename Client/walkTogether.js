function getWalkingPlayerOffset(player, ang, position) {
    /*
    * Given an angle (facing angle) and position, this method returns offset for walk.
    * If position is left, offset of left position of player is returned.
    * Otherwise, by default it returns right position offset.
    */
    let x, y;
    if(ang >= 0 && ang <= 90) {
        y = ang/90
        x = 1 - y
    }
    else if(ang > 90 && ang <= 180) {
        x = ((ang-90)/90) * -1;
        y = 1 + x;
    }
    else if(ang > 180 && ang <= 270) {
        y = ((ang-180)/90) * -1;
        x = (1 + y) * -1;
    }
    else if(ang > 270 && ang <= 360) {
        x = ((ang-270)/90)
        y = ( 1 - x ) * -1 
    }
    if(position === "left") {
        x = -x;
        y = -y;
    }

    return player.getOffsetFromGivenWorldCoords(player.position.x + x, player.position.y + y, player.position.z);
}

// Events.

mp.events.add("detach", (target) => {
    /*
    * Detach an attached entity.
    */
    target.detach(true, false)
})

mp.events.add("attach", (player, target, position) => {
    /*
    * Attach target to current player after calculating the correct offset.
    */
    const ang = player.getHeading()
    target.setHeading(ang);
    const offset = getWalkingPlayerOffset(player, ang, position)
    target.attachTo(player.handle, 23553, offset.x, offset.y, 0, 0, 0, 0, true, true, true, false, 0, false);
})

mp.events.add('entityStreamIn', (entity) => {
    /*
    * Attach walking players for a newly streamed in player.
    */
    if (entity == null)
        return;
    if (entity.type !== "player") return;
    if(mp.players.local.getVariable("walkingWith")) {
        mp.events.callRemote("attachWalkingPlayers", entity)
    }
});

mp.events.add('entityStreamOut', (entity) => {
    /*
    * Detach walking players for streaming out player.
    */
    if (entity == null)
        return;
    if (entity.type !== "player") return;
    if(mp.players.local.getVariable("walkingWith")) {
        mp.events.callRemote("detachWalkingPlayers", entity);
    }
});

// Added these 2 events to play walking animation again once a user streams in.

mp.events.add("play_walking_anim", (entity, animDictionary, animName) => {
    mp.game.streaming.requestAnimDict(animDictionary);
    new Promise(() => {
        const timer = setInterval(() => {
            if(mp.game.streaming.hasAnimDictLoaded(animDictionary)) {
                clearInterval(timer);
                resolve(entity, animDictionary, animName);
            }
        }, 100);
    });
});

function resolve(entity, animDictionary, animName)
{
    entity.taskPlayAnim(animDictionary, animName, 8.0, 0.0, -1, 1, 0.0, false, false, false);
}
