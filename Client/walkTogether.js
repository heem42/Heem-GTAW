var isWalking = false;
var walkFunc = null;
var walkingWith = null;

function getWalkingPlayerOffset(ang, position) {
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

    return mp.players.local.getOffsetFromGivenWorldCoords(mp.players.local.position.x + x, mp.players.local.position.y + y, mp.players.local.position.z);
}

function getFacingAngle(direction) {
    /*
    * This method returns the angle (facing angle) given direction vector.
    * It calculates the angle using the direction coordinates using trigonometry.
    */
    const cam_x = direction.x;
    const cam_y = direction.y;

    let ang = 0;
    if(cam_x >= 0 && cam_x <= 1) {
        if(cam_y >= 0 && cam_y <= 1) {
            ang = 360 - (Math.asin(cam_x) * (180/Math.PI));
        }
        else {
            ang = Math.asin(cam_x) * (180/Math.PI) + 180;
        }
    }
    else {
        ang = Math.acos(cam_y) * (180/Math.PI);
    }

    return ang;
}

function walkSync() {
    /*
    * This method sets player's facing angle calculated by getFacingAngle method.
    */
    if(isWalking) {
        const camera = mp.cameras.new("gameplay");
        const cam_z = camera.getDirection().z;
        if(Math.abs(cam_z) < 0.2) {
            const ang = getFacingAngle(camera.getDirection())
            mp.players.local.setHeading(ang);
            walkingWith.setHeading(ang);
        }
    }
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
    const camera = mp.cameras.new("gameplay");
    const cam_x = camera.getDirection().x;
    const cam_y = camera.getDirection().y;
    const ang = player.getHeading()
    const offset = getWalkingPlayerOffset(ang, position)
    
    target.attachTo(player.handle, 23553, offset.x, offset.y, 0, cam_x, cam_y, 0, true, true, true, false, 0, false);
})

mp.events.add("attachedTo", (p, position) => {
    /*
    * Attach current player to target after calculating correct offset.
    */
    const ang = p.getHeading()
    mp.players.local.setHeading(ang);
    const offset = getWalkingPlayerOffset(ang, position)
    
    mp.players.local.attachTo(p.handle, 23553, offset.x, offset.y, 0, 0, 0, 0, true, true, true, false, 0, false);
})

mp.events.add("beginCameraControl", (target) => {
    /*
    * Sets variables and interval method for controlling direction with mouse.
    */
    isWalking = true;
    walkingWith = target;
    walkFunc = setInterval(walkSync, 50)
})

mp.events.add("stopCameraControl", () => {
    /*
    * Clears variables and interval method for controlling direction with mouse.
    */
    isWalking = false;
    walkingWith = null;
    clearInterval(walkFunc)
    walkFunc = null;
})
