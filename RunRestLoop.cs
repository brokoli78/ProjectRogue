using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectRogue
{
    public static class RunRestLoop
    {
        static List<Tile> path;
        public static bool runLoop = false;
        public static bool restLoop = false;
        static int turnsRested = 0;
        static Player player;
        static Tile destination;
        static bool pickup = false;
        static TileMap map;
        static List<Message> runRestStopMessage = new List<Message> 
        { 
            Messages.enemyCloseMessage(null),
            Messages.hpRestored()
        };
        public static bool cont = false;
        public static bool start = true;

        static bool hpRegen = false;

        static bool explore = false;

        public static void RunLoop()
        {
            if (explore && !runLoop)
                explore = false;

            if (runLoop)
            {
                List<Monster> monsters = player.FieldOfVision().Where(x => x.creature as Monster != null).Select(x => (Monster)x.creature).ToList();
                if (monsters.Count > 0)
                {
                    foreach (Monster monster in monsters)
                    {
                        GameLog.newMessage(Messages.enemyCloseMessage(monster));
                    }
                }

                List<Message> newMessages = GameLog.getAndClearNewMessages();

                if (newMessages.Count > 0)
                {
                    foreach (Message m1 in newMessages)
                    {
                        foreach (Message m2 in runRestStopMessage)
                        {
                            if (Message.sameBaseMessage(m1, m2))
                            {
                                runLoop = false;
                                explore = false;
                                break;
                            }
                        }

                        if (runLoop == false)
                        {
                            break;
                        }
                    }
                }
            }
            
            if (runLoop && explore)
            {
                if(cont)
                {
                    GameLog.newMessage("You continue exploring.");
                    cont = false;
                }
                if (start)
                {
                    GameLog.newMessage("You start exploring.");
                    start = false;
                }

                Tile tile = path.Last();
                if (destination.wasVisible == true && !pickup)
                {
                    runLoop = false;
                    explore = false;
                    StartExploreLoop();
                    RunLoop();
                    return;
                }

                if (tile.walkable)
                {
                    player.move(tile.x, tile.y);
                    GameObject.newTurn();
                }
                else
                {
                    runLoop = false;
                    explore = false;
                    return;
                }

                path.RemoveAt(path.Count - 1);
                if (path.Count == 0)
                {
                    runLoop = false;
                    explore = false;

                    if (destination.items.Where(i => i.tags.Contains("autopickup")).Count() > 0)
                    {
                        destination.items.Where(i => i.tags.Contains("autopickup")).ElementAt(0).PickUp(GameController.player);
                        GameObject.newTurn();
                    }

                    StartExploreLoop();
                    RunLoop();
                    return;
                }
            }
            else if(runLoop) //TODO: multilevel stuff
            {
                Tile tile = path.Last();

                if (tile.walkable)
                {
                    player.move(tile.x, tile.y);
                    GameObject.newTurn();
                }
                else
                {
                    runLoop = false;
                    explore = false;
                    return;
                }

                path.RemoveAt(path.Count - 1);
                if (path.Count == 0)
                {
                    runLoop = false;
                    explore = false;
                }

            }
        }

        public static void StartRunLoop(List<Tile> path)
        {
            explore = false;
            player = GameController.player;
            map = GameController.map;

            List<Monster> monsters = player.FieldOfVision().Where(x => x.creature as Monster != null).Select(x => (Monster)x.creature).ToList();
            if (monsters.Count > 0)
            {
                GameLog.newMessage(Messages.enemiesNearby());
                return;
            }

            RunRestLoop.path = path;

            runLoop = true;
        }

        public static void StartExploreLoop(Player p, TileMap m)
        {
            player = p;
            map = m;

            List<Monster> monsters = player.FieldOfVision().Where(x => x.creature as Monster != null).Select(x => (Monster)x.creature).ToList();
            if (monsters.Count > 0)
            {
                GameLog.newMessage(Messages.enemiesNearby());
                return;
            }

            if (map[player.x, player.y].items.Where(i => i.tags.Contains("autopickup")).Count() > 0)
            {
                map[player.x, player.y].items.Where(i => i.tags.Contains("autopickup")).ElementAt(0).PickUp(GameController.player);
                GameObject.newTurn();
                StartExploreLoop();
                return;
            }

            path = new List<Tile>();

            if (AStar.ExplorePath(map, map[player.x, player.y], out path, true))
            {
                destination = path.First();
                pickup = destination.wasVisible;
                runLoop = true;
                explore = true;
            }
            else
            {
                GameLog.newMessage("Done exploring.");
            }
        }


        private static void StartExploreLoop()
        {
            if (map[player.x, player.y].items.Where(i => i.tags.Contains("autopickup")).Count() > 0)
            {
                map[player.x, player.y].items.Where(i => i.tags.Contains("autopickup")).ElementAt(0).PickUp(GameController.player);
                GameObject.newTurn();
                StartExploreLoop();
                return;
            }

            path = new List<Tile>();

            if (AStar.ExplorePath(map, map[player.x, player.y], out path, true))
            {
                destination = path.First();
                pickup = destination.wasVisible;
                runLoop = true;
                explore = true;
            }
            else
            {
                GameLog.newMessage("Done exploring.");
            }
        }

        public static void RestLoop()
        {
            if(restLoop)
            {

                if(player.health == player.maxHealth && hpRegen)
                {
                    GameLog.newMessage(Messages.hpRestored());
                    hpRegen = false;
                }

                List<Monster> monsters = player.FieldOfVision().Where(x => x.creature as Monster != null).Select(x => (Monster)x.creature).ToList();
                if (monsters.Count > 0)
                {
                    foreach (Monster monster in monsters)
                    {
                        GameLog.newMessage(Messages.enemyCloseMessage(monster));
                    }
                }

                List<Message> newMessages = GameLog.getAndClearNewMessages();

                if (newMessages.Count > 0)
                {
                    foreach (Message m1 in newMessages)
                    {
                        foreach (Message m2 in runRestStopMessage)
                        {
                            if (Message.sameBaseMessage(m1, m2))
                            {
                                restLoop = false;
                                break;
                            }
                        }

                        if (runLoop == false)
                        {
                            break;
                        }
                    }
                }

                if(restLoop)
                {
                    if(turnsRested < 100)
                    {
                        GameObject.newTurn();
                        turnsRested++;
                    }
                    else
                    {
                        restLoop = false;
                    }
                }

            }
        }

        public static void StartRestLoop(Player p, TileMap m)
        {
            player = p;
            map = m;

            if (player.maxHealth != player.health)
                hpRegen = true;

            List<Monster> monsters = player.FieldOfVision().Where(x => x.creature as Monster != null).Select(x => (Monster)x.creature).ToList();
            if (monsters.Count > 0)
            {
                GameLog.newMessage(Messages.enemiesNearby());
                return;
            }

            restLoop = true;
            turnsRested = 0;
        }

    }
}
