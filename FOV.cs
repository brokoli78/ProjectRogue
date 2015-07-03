using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace ProjectRogue
{
    public static class FOV
    {
        enum visibilityStatus { outside, between, shallowBump, steepBump, blocking }

#if(FOV)
        public static List<Line> linesToDraw = new List<Line>();
#endif

        public static List<Tile> FieldOfVision(Tile source, int visionRadius, TileMap map)
        {
            if (!source.Translucent)
                return null;

            List<Tile> visibleTiles = new List<Tile>();

            // create the first quadrant

            List<Tile> quadrant = new List<Tile>();

            visibleTiles.AddRange(CalculateQuadrant(source, visionRadius, map, new Line(1, 0, 0, 1)));
            visibleTiles.AddRange(CalculateQuadrant(source, visionRadius, map, new Line(0, 0, 1, 1)));
            visibleTiles.AddRange(CalculateQuadrant(source, visionRadius, map, new Line(0, 1, 1, 0)));
            visibleTiles.AddRange(CalculateQuadrant(source, visionRadius, map, new Line(1, 1, 0, 0)));

            visibleTiles = visibleTiles.Distinct().ToList();

            visibleTiles.Add(source);
            return visibleTiles;
        }

        private static List<Tile> CalculateQuadrant(Tile source, int visionRadius, TileMap map, Line sourceVisionLine)
        {

            List<Tile> quadrant = new List<Tile>();
            List<Tile> visibleTiles = new List<Tile>();

            int xFactor = (int)(sourceVisionLine.start.x * 2 - 1);
            int yFactor = (int)(sourceVisionLine.end.y * 2 - 1);

            for (int x = 1; x <= visionRadius / Math.Cos(45); x++)
            {
                for (int y = 0; y <= x; y++)
                {
                    if (!(visionRadius < Math.Sqrt(Math.Pow(x - y, 2) + Math.Pow(y, 2))))
                    {
                        if (map.inMap(source.x + xFactor * (x - y), source.y + yFactor * y))
                        {
                            quadrant.Add(map[source.x + xFactor * (x - y), source.y + yFactor * y]);
                        }
                    }
                }
            }

#if(FOV)
            for (int i = 0; i < quadrant.Count; i++)
                quadrant[i].num = i;
#endif

            List<View> views = new List<View>();
            views.Add(new View(
                source.x + (int) sourceVisionLine.end.x,
                source.y + (int) sourceVisionLine.end.y, 
                source.x + xFactor * visionRadius + (int) sourceVisionLine.start.x, 
                source.y + (int) sourceVisionLine.start.y,
                source.x + (int) sourceVisionLine.start.x,
                source.y + (int) sourceVisionLine.start.y, 
                source.x + (int) sourceVisionLine.end.x, 
                source.y + yFactor * visionRadius + (int) sourceVisionLine.end.y)); // create shallow and steep initial lines
             
            for (int i = 0; i < quadrant.Count; i++)
            {
                List<View> currentViews = new List<View>();
                currentViews.AddRange(views);

                // fov calculation
                while (currentViews.Count > 0)
                {

                    int j = views.FindIndex(x => x.Equals(currentViews[0]));
                    visibilityStatus? vis = getVisibilityStatus(quadrant[i], source, map, currentViews.ElementAt(0), sourceVisionLine);
                    switch (vis)
                    {
                        case visibilityStatus.outside:
                            //Debug.WriteLine("Outside");
                            //do nothing
                            //quadrant[i].debug = Color.Gold;
                            break;

                        case visibilityStatus.between:
                            //Debug.WriteLine("Between");
#if(FOV)
                            quadrant[i].debug = Color.Green;
#endif
                            visibleTiles.Add(quadrant[i]);
                            if (!quadrant[i].Translucent)
                            {

                                Line possibleNewShallow = new Line(views[j].shallow.start, quadrant[i].position + sourceVisionLine.end);

                                View view = new View(possibleNewShallow, currentViews[0].steep);

                                foreach (Tile tile in View.steepBump)
                                {
                                    if (obstructionCheck(tile, source, view.shallow, sourceVisionLine))
                                    {
                                        view.shallow = new Line(tile.position + sourceVisionLine.start, view.shallow.end);
                                    }
                                }

                                if (!Line.sameLine(view.shallow, currentViews[0].steep))
                                {
                                    views.Add(view);
#if(FOV)
                                    linesToDraw.Add(view.shallow);
                                    linesToDraw.Add(view.steep);
#endif
                                }


                                Line possibleNewSteep = new Line(views[j].steep.start, quadrant[i].position + sourceVisionLine.start);

                                view = new View(currentViews[0].shallow, possibleNewSteep);

                                foreach (Tile tile in View.shallowBump)
                                {
                                    if (obstructionCheck(tile, source, view.steep, sourceVisionLine))
                                    {
                                        view.steep = new Line(tile.position + sourceVisionLine.end, view.steep.end);
                                    }
                                }

                                if (!Line.sameLine(currentViews[0].shallow, view.steep))
                                {
                                    views.Add(view);
#if(FOV)
                                    linesToDraw.Add(view.shallow);
                                    linesToDraw.Add(view.steep);
#endif
                                }

                                View.shallowBump.Add(quadrant[i]);
                                View.steepBump.Add(quadrant[i]);

                                views.RemoveAt(j);
                            }
                            break;

                        case visibilityStatus.blocking:
                            //Debug.WriteLine("Blocking");
#if(FOV)
                            quadrant[i].debug = Color.Indigo;
#endif
                            visibleTiles.Add(quadrant[i]);
                            if (!quadrant[i].Translucent)
                            {
                                views.RemoveAt(j);
                            }
                            break;

                        case visibilityStatus.shallowBump:
                            //Debug.WriteLine("Shallow");
#if(FOV)
                            quadrant[i].debug = Color.Orange;
#endif
                            visibleTiles.Add(quadrant[i]);
                            if (!quadrant[i].Translucent)
                            {
                                views[j].shallow = new Line(views[j].shallow.start, quadrant[i].position + sourceVisionLine.end);

                                foreach (Tile tile in View.steepBump)
                                {
                                    if (obstructionCheck(tile, source, views[j].shallow, sourceVisionLine))
                                    {
#if(FOV)
                                        tile.debug = Color.Firebrick;
                                        quadrant[i].debug = Color.DarkRed;
#endif
                                        views[j].shallow = new Line(tile.position + sourceVisionLine.start, views[j].shallow.end);
                                    }
                                }

                                if (Line.sameLine(views[j].shallow, views[j].steep))
                                {
                                    views.RemoveAt(j);
                                }
                                else
                                {
#if(FOV)
                                    linesToDraw.Add(views[j].shallow);
                                    linesToDraw.Add(views[j].steep);
#endif
                                }
                                View.shallowBump.Add(quadrant[i]);
                            }
                            break;

                        case visibilityStatus.steepBump:
                            //Debug.WriteLine("Steep");
#if(FOV)
                            quadrant[i].debug = Color.Aqua;
#endif
                            visibleTiles.Add(quadrant[i]);
                            if (!quadrant[i].Translucent)
                            {
                                views[j].steep = new Line(views[j].steep.start, quadrant[i].position + sourceVisionLine.start);

                                foreach (Tile tile in View.shallowBump)
                                {
                                    if (obstructionCheck(tile, source, views[j].steep, sourceVisionLine))
                                    {
#if(FOV)
                                        tile.debug = Color.Bisque;
                                        quadrant[i].debug = Color.Brown;
#endif
                                        views[j].steep = new Line(tile.position + sourceVisionLine.end, views[j].steep.end);
                                    }
                                }


                                if (Line.sameLine(views[j].shallow, views[j].steep))
                                {
                                    views.RemoveAt(j);
                                }
                                else
                                {
#if(FOV)
                                    linesToDraw.Add(views[j].shallow);
                                    linesToDraw.Add(views[j].steep);
#endif
                                }

                                View.steepBump.Add(quadrant[i]);
                            }
                            break;

                        default:
                            throw new ArgumentException("visibility status is null!!");
                    }

                    currentViews.Remove(currentViews[0]);
                }
            }

            View.Reset();

            return visibleTiles;
        }

        static bool obstructionCheck(Tile tile, Tile source, Line newLine, Line sourceVisionLine)
        {
            int factor = (int)(sourceVisionLine.end.y * 2 - 1);
            Vector2 topEdge = tile.position + sourceVisionLine.end;
            Vector2 bottomEdge = tile.position + sourceVisionLine.start;

            Line objectLine = new Line(topEdge, bottomEdge);

            Vector2 intersect = Line.Intersect(objectLine, newLine);

            return intersect.y * factor > bottomEdge.y * factor && intersect.y * factor < topEdge.y * factor;

        }

        static visibilityStatus? getVisibilityStatus(Tile tile, Tile source, TileMap map, View view, Line sourceVisionLine) 
        {
            int factor = (int) (sourceVisionLine.end.y * 2 - 1);

            Vector2 convertedTile = tile.position - source.position;
            Vector2 convertedTopEdge = convertedTile + sourceVisionLine.end;
            Vector2 convertedBottomEdge = convertedTile + sourceVisionLine.start;
            Line convertedObjectLine = new Line(convertedTopEdge, convertedBottomEdge);

            Line convertedSteep = new Line(view.steep.start - source.position, view.steep.end - source.position);
            Line convertedShallow = new Line(view.shallow.start - source.position, view.shallow.end - source.position);

            Vector2 steepIntersect = Line.Intersect(convertedObjectLine, convertedSteep);
            Vector2 shallowIntersect = Line.Intersect(convertedObjectLine, convertedShallow);

            if (steepIntersect.y * factor <= convertedBottomEdge.y * factor || shallowIntersect.y * factor >= convertedTopEdge.y * factor) 
            {
                return visibilityStatus.outside;
            }

            if (shallowIntersect.y * factor > convertedBottomEdge.y * factor && shallowIntersect.y * factor < convertedTopEdge.y * factor)
            {
                return visibilityStatus.shallowBump;
            }

            if (steepIntersect.y * factor > convertedBottomEdge.y * factor && steepIntersect.y * factor < convertedTopEdge.y * factor)
            {
                return visibilityStatus.steepBump;
            }

            if (steepIntersect.y * factor >= convertedTopEdge.y * factor && shallowIntersect.y * factor <= convertedBottomEdge.y * factor)
            {
                return visibilityStatus.between;
            }

            if (steepIntersect.y * factor < convertedTopEdge.y * factor && shallowIntersect.y * factor > convertedBottomEdge.y * factor)
            {
                return visibilityStatus.blocking;
            }

            return null;

        }
    }

        class View
        {
            internal Line shallow;
            internal Line steep;
            internal static List<Tile> shallowBump = new List<Tile>();
            internal static List<Tile> steepBump = new List<Tile>();

            internal View(Line shallow, Line steep)
            {
                this.shallow = shallow;
                this.steep = steep;
            }

            internal View(int shallowStartX, int shallowStartY, int shallowEndX, int shallowEndY, int steepStartX, int steepStartY, int steepEndX, int steepEndY)
            {
                this.shallow = new Line(shallowStartX, shallowStartY, shallowEndX, shallowEndY);
                this.steep = new Line(steepStartX, steepStartY, steepEndX, steepEndY);
            }

            internal static void Reset()
            {
                shallowBump = new List<Tile>();
                steepBump = new List<Tile>();
            }
            
        }

}
