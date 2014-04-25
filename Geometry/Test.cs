using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RFC.Geometry
{
    static class GeomTest
    {
        static void Main()
        {
            Line n = new Line(new Vector2(0, -5), new Vector2(0, 5));
            Line e = new Line(new Vector2(-5, 0), new Vector2(5, 0));

            Circle origCirc = new Circle(new Vector2(0, 0), 1);
            Circle nCirc = new Circle(new Vector2(0, 5), 1);
            Circle nnCirc = new Circle(new Vector2(0, 10), 1);
            Circle eCirc = new Circle(new Vector2(5, 0), 1);
            Circle eeCirc = new Circle(new Vector2(10, 0), 1);
            Circle sCirc = new Circle(new Vector2(0, -5), 1);
            Circle ssCirc = new Circle(new Vector2(0, -10), 1);
            Circle wCirc = new Circle(new Vector2(-5, 0), 1);
            Circle wwCirc = new Circle(new Vector2(-10, 0), 1);

            Console.WriteLine(LineCircleIntersection.Intersection(n, nnCirc, 0));
            Console.WriteLine(LineCircleIntersection.Intersection(n, nnCirc, 1));

            Console.WriteLine("-----------------");
            Console.WriteLine(LineCircleIntersection.Intersection(n, nCirc, 0));
            Console.WriteLine(LineCircleIntersection.Intersection(n, nCirc, 1));

            Console.WriteLine("-----------------");
            Console.WriteLine(LineCircleIntersection.Intersection(n, origCirc, 0));
            Console.WriteLine(LineCircleIntersection.Intersection(n, origCirc, 1));

            Console.WriteLine("-----------------");
            Console.WriteLine(LineCircleIntersection.Intersection(n, sCirc, 0));
            Console.WriteLine(LineCircleIntersection.Intersection(n, sCirc, 1));

            Console.WriteLine("-----------------");
            Console.WriteLine(LineCircleIntersection.Intersection(n, ssCirc, 0));
            Console.WriteLine(LineCircleIntersection.Intersection(n, ssCirc, 1));
            Console.WriteLine("-----------------");
            Console.WriteLine(LineCircleIntersection.Intersection(e, eeCirc, 0));
            Console.WriteLine(LineCircleIntersection.Intersection(e, eeCirc, 1));
            Console.WriteLine("-----------------");
            Console.WriteLine(LineCircleIntersection.Intersection(e, eCirc, 0));
            Console.WriteLine(LineCircleIntersection.Intersection(e, eCirc, 1));

            Console.WriteLine("-----------------");
            Console.WriteLine(LineCircleIntersection.Intersection(e, origCirc, 0));
            Console.WriteLine(LineCircleIntersection.Intersection(e, origCirc, 1));

            Console.WriteLine("-----------------");
            Console.WriteLine(LineCircleIntersection.Intersection(e, wCirc, 0));
            Console.WriteLine(LineCircleIntersection.Intersection(e, wCirc, 1));

            Console.WriteLine("-----------------");
            Console.WriteLine(LineCircleIntersection.Intersection(e, wwCirc, 0));
            Console.WriteLine(LineCircleIntersection.Intersection(e, wwCirc, 1));

        }
    }
}
