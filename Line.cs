using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectRogue
{
    public class Line
    {
        private Vector2 Start;
        public Vector2 start
        {
            get { return Start; }
            private set { Start = value; }
        }

        private Vector2 End;
        public Vector2 end
        {
            get { return End; }
            private set { End = value; }
        }

        private Vector2 Direction;
        public Vector2 direction
        {
            get { return Direction; }
            private set { Direction = value; }
        }


        public Line(Vector2 start, Vector2 end)
        {
            this.start = start;
            this.end = end;

            this.direction = end - start;
        }

        public Line(double startX, double startY, double endX, double endY)
        {
            this.start = new Vector2(startX, startY);
            this.end = new Vector2(endX, endY);

            this.direction = end - start;
        }

        public Line(Vector2 start, double endX, double endY)
        {
            this.start = start;
            this.end = new Vector2(endX, endY);

            this.direction = end - start;
        }

        public static Vector2 Intersect(Line firstLine, Line secondLine)
        {
            /*if (firstLine.direction.length() == 0 || secondLine.direction.length() == 0)
                throw new ArgumentException("Cannot intersect lines without direction!!!!!!!!!!!!!!!!!!!!!!");*/

            if (!Vector2.LinearIndependent(firstLine.direction, secondLine.direction))
                return null; // parallel lines

            Vector2 a = firstLine.start;            //a + r * b = c + k * d
            Vector2 b = firstLine.direction;
            Vector2 c = secondLine.start;
            Vector2 d = secondLine.direction;
            double r;

            if (d.y != 0)
            {
                r = (a.x - c.x - ((a.y - c.y) * d.x) / d.y) /
                    ((b.y * d.x) / d.y - b.x);
            }
            else
            {
                r = (c.y - a.y) / b.y;
            }

            return a + (r * b);
        }

        public static bool areParallel(Line line1, Line line2)
        {
            return !Vector2.LinearIndependent(line1.direction, line2.direction);
        }

        public static bool sameLine(Line line1, Line line2)
        {
            return !Vector2.LinearIndependent(line2.start - line1.start, line1.direction) && !Vector2.LinearIndependent(line2.start - line1.start, line2.direction);
        }
    }

}
