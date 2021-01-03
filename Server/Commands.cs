using System;
using GTANetworkAPI;

namespace GTAW.Player
{
    class Commands : Script
    {
        const int FULL_BODY_CONTROLLABLE = 47;
        const string WALK_ANIM_DICT = "anim@move_m@grooving@";
        const string ANIAMTION_IN_DICT = "walk";

        const double WALK_WITH_MINIMUM_DISTANCE = 2.0;

        const string WALK_OFFER_KEY = "walkOfferWith";
        const string WALKING_WITH_KEY = "walkingWith";
        const string WALK_POSITION_KEY = "walkPosition";
        const string WALK_CONTROLLER_KEY = "walkController";
        const string WALK_MOUSE_MODE_KEY = "walkMouseMode";

        public bool isWalking(Client player)
        {
            /*
             * Checks if current user is walking or not.
            */
            if (player.HasData(WALKING_WITH_KEY)) return true;
            return false;
        }
        
        [Command("walkwith")]
        public void walkWith(Client player, Client target, string position="right")
        {
            /*
             * A command that sends an invite to walk together with target player.
             * Position can be either "left" or "right"
             */
            float distance = (player.Position - target.Position).Length();
            string[] validPositions = { "left", "right" };

            if(Array.IndexOf(validPositions, position) == -1)
            {
                player.SendChatMessage("Valid positions are either left or right.");
            }
            else if(distance > WALK_WITH_MINIMUM_DISTANCE)
            {
                player.SendChatMessage($"You are not close enough to {target.Name}");
            }
            else if(isWalking(player) || isWalking(target))
            {
                player.SendChatMessage("Either you or the target player is already walking with someone!");
            }
            else
            {
                target.SendChatMessage($"{player.Name} has offered you to walk together. Use /acceptwalk to accept the offer.");
                player.SendChatMessage($"You have sent an offer to walk together to {target.Name}");

                target.SetData(WALK_OFFER_KEY, player);
                target.SetData(WALK_POSITION_KEY, position);
            }
        }

        [Command("acceptwalk")]
        public void acceptWalk(Client player)
        {
            /*
             * A command to accept walk together invite.
             * If user was offered to walk with, then play animation for both players.
             * Invoke client side methods to attach both players at specific offset.
             */
            if(isWalking(player))
            {
                player.SendChatMessage("You are already walking with someone!");
            }
            else if(!player.HasData(WALK_OFFER_KEY))
            {
                player.SendChatMessage("Nobody has offered you to walk with them!");
            }
            else
            {
                Client target = player.GetData(WALK_OFFER_KEY);
                string position = player.GetData(WALK_POSITION_KEY);
                float distance = (player.Position - target.Position).Length();

                if (distance <= WALK_WITH_MINIMUM_DISTANCE)
                {

                    player.PlayAnimation(WALK_ANIM_DICT, ANIAMTION_IN_DICT, FULL_BODY_CONTROLLABLE);
                    target.PlayAnimation(WALK_ANIM_DICT, ANIAMTION_IN_DICT, FULL_BODY_CONTROLLABLE);

                    NAPI.ClientEvent.TriggerClientEvent(target, "attach", target, player, position);
                    NAPI.ClientEvent.TriggerClientEvent(player, "attachedTo", target, position);

                    target.SendChatMessage($"{player.Name} has accepted your walk offer. [/stopwalk to stop]");
                    player.SendChatMessage($"You have accepted to walk with {target.Name}. [/stopwalk to stop]");

                    player.ResetData(WALK_OFFER_KEY);
                    player.ResetData(WALK_POSITION_KEY);
                    target.SetData(WALKING_WITH_KEY, player);
                    player.SetData(WALKING_WITH_KEY, target);
                    target.SetData(WALK_CONTROLLER_KEY, true);

                }
                else
                {
                    player.SendChatMessage($"You are not close enough to {target.Name}");
                }
            }
        }

        [Command("changewalkcontrol")]
        public void changeWalkControl(Client player)
        {
            /*
             * If current user is controlling a walk, he's able to change the control mode of walk.
             * For example, if walk direction was controlled by keys W/A/S/D then it will toggle to Mouse.
             * Otherwise, back to keyboard control.
             */
            if(player.HasData(WALK_CONTROLLER_KEY) && isWalking(player))
            {
                if (!player.HasData(WALK_MOUSE_MODE_KEY))
                {
                    NAPI.ClientEvent.TriggerClientEvent(player, "beginCameraControl", player.GetData(WALKING_WITH_KEY));
                    player.SetData(WALK_MOUSE_MODE_KEY, true);
                    player.SendChatMessage("You will now be able to control the direction with mouse.");
                }
                else
                {
                    NAPI.ClientEvent.TriggerClientEvent(player, "stopCameraControl");
                    player.ResetData(WALK_MOUSE_MODE_KEY);
                    player.SendChatMessage("You will now be able to control walk with keyboard.");
                }
            }
            else
            {
                player.SendChatMessage("You are not the controller of walk right now.");
            }
        }

        [Command("stopwalk")]

        public void stopWalk(Client player)
        {
            /*
             * Stops the walk. Can be invoked by any of player (player or target)
             */
            if(player.HasData(WALKING_WITH_KEY))
            {
                Client target = player.GetData(WALKING_WITH_KEY);

                player.StopAnimation();
                target.StopAnimation();

                NAPI.ClientEvent.TriggerClientEvent(player, "detach", target);
                NAPI.ClientEvent.TriggerClientEvent(target, "detach", player);

                target.SendChatMessage($"{player.Name} has stopped walking with you.");
                player.SendChatMessage($"You have stopped walking with {target.Name}");

                player.ResetData(WALKING_WITH_KEY);
                target.ResetData(WALKING_WITH_KEY);
                player.ResetData(WALK_CONTROLLER_KEY);
                target.ResetData(WALK_CONTROLLER_KEY);

                NAPI.ClientEvent.TriggerClientEvent(player, "stopCameraControl");
                NAPI.ClientEvent.TriggerClientEvent(target, "stopCameraControl");
            }
            else
            {
                player.SendChatMessage("You are not currently walking with anyone!");
            }
        }
    }
}
