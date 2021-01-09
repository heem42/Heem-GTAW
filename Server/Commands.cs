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

        public bool isWalking(Client player)
        {
            /*
             * Checks if current user is walking or not.
            */
            if (player.HasData(WALKING_WITH_KEY)) return true;
            return false;
        }

        [RemoteEvent("attachWalkingPlayers")]
        public void attachWalkingPlayers(Client sender, Client entity)
        {
            /*
             * Remote event for attaching walking players for newly streamed in player
             * entity -> Newly streamed in client.
             * player1 -> Player controlling the walk.
             * player2 -> Player being attached to player1.
             * positon -> Position to be attached (left/right)
             */
            NAPI.ClientEvent.TriggerClientEvent(entity, "attach", sender, sender.GetSharedData("walkingWith"), sender.GetSharedData("walkingPosition"));
            NAPI.ClientEvent.TriggerClientEvent(entity, "play_walking_anim", sender, WALK_ANIM_DICT, ANIAMTION_IN_DICT);
            NAPI.ClientEvent.TriggerClientEvent(entity, "play_walking_anim", sender.GetSharedData("walkingWith"), WALK_ANIM_DICT, ANIAMTION_IN_DICT);
        }

        [RemoteEvent("detachWalkingPlayers")]
        public void detachWalkingPlayers(Client sender, Client entity)
        {
            /*
             * Remote event for detaching walking players for streaming out player.
             * entity -> Streamed out client.
             * player -> Player being detached.
             */
            NAPI.ClientEvent.TriggerClientEvent(entity, "detach", sender);
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
            else if(player == target)
            {
                player.SendChatMessage("You can not walk with yourself!");
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
                acceptWalk(target);
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

                    NAPI.ClientEvent.TriggerClientEventForAll("attach", target, player, position);

                    target.SetSharedData("walkingWith", player);
                    target.SetSharedData("walkingPosition", position);

                    target.SendChatMessage($"{player.Name} has accepted your walk offer. [/stopwalk to stop]");
                    player.SendChatMessage($"You have accepted to walk with {target.Name}. [/stopwalk to stop]");

                    player.ResetData(WALK_OFFER_KEY);
                    player.ResetData(WALK_POSITION_KEY);
                    target.SetData(WALKING_WITH_KEY, player);
                    player.SetData(WALKING_WITH_KEY, target);
                }
                else
                {
                    player.SendChatMessage($"You are not close enough to {target.Name}");
                }
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

                NAPI.ClientEvent.TriggerClientEventForAll("detach", target);

                target.ResetSharedData("walkingWith");
                target.ResetSharedData("walkingPosition");
                player.ResetSharedData("walkingWith");
                player.ResetSharedData("walkingPosition");

                target.SendChatMessage($"{player.Name} has stopped walking with you.");
                player.SendChatMessage($"You have stopped walking with {target.Name}");

                player.ResetData(WALKING_WITH_KEY);
                target.ResetData(WALKING_WITH_KEY);
            }
            else
            {
                player.SendChatMessage("You are not currently walking with anyone!");
            }
        }
    }
}
