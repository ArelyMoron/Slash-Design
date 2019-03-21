using System;
using System.Windows;
using System.Windows.Media;
using System.Runtime.InteropServices;
// elaborado por Arely M.
namespace Slash_Design
{
    static class ColorTools
    {
        public static Color ActualColor { get; set; }
        public static Color LastColor { get; set; }
        private static Color[] LightShades;
        private static Color[] DarkShades;

        // ocupo estas librerias para obtener la informacion que necesito de la pantalla como un pixel o el escritorio
        [DllImport("user32.dll")]
        static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("user32.dll")]
        static extern Int32 ReleaseDC(IntPtr hwnd, IntPtr hdc);

        [DllImport("gdi32.dll")]
        static extern uint GetPixel(IntPtr hdc, int nXPos, int nYPos);

        public static HSL ToHsl(this Color color) // paso de color rgb a hsl 
        {
            double double_l, double_s, double_h;
            byte l, s;
            int h;
            double r = color.R / 255.0;
            double g = color.G / 255.0;
            double b = color.B / 255.0;

            double max = r;
            if (max < g)
                max = g;
            if (max < b)
                max = b;

            double min = r;
            if (min > g)
                min = g;
            if (min > b)
                min = b;

            double diff = max - min;
            double_l = (max + min) / 2;
            if (Math.Abs(diff) < 0)
            {
                double_h = 0;
                double_s = 0;
            }
            else
            {
                if (double_l <= 0.5)
                    double_s = diff / (max + min);
                else
                    double_s = diff / (2 - max - min);

                double r_dist = (max - r) / diff;
                double g_dist = (max - g) / diff;
                double b_dist = (max - b) / diff;
                if (r == max)
                    double_h = b_dist - g_dist;
                else if (g == max)
                    double_h = 2 + r_dist - b_dist;
                else
                    double_h = 4 + g_dist - r_dist;

                double_h = double_h * 60;
                if (double_h < 0)
                    double_h += 360;
            }
            h = Convert.ToInt32(double_h);
            s = Convert.ToByte(double_s * 100);
            l = Convert.ToByte(double_l * 100);
            HSL hsl = HSL.FromHSL(h, s, l);
            return hsl;
        }

        public static string ToHex(this Color color, bool isAlpha) // convierto el rgb a hexadecimal especificando si tiene alfa o no
        {
            if (!isAlpha)
                return color.ToString().Remove(1, 2);
            else
                return color.ToString();
        }

        public static Color FromHex(string hex)
        {
            if (!(hex[0] == '#'))
            {
                hex.Insert(0, "#");
            }

            try
            {
                return (Color)ColorConverter.ConvertFromString(hex);
            }
            catch (FormatException)
            {
                return LastColor;
            }
        }

        public static Color[] GetLightShades(Color color) // obtengo las shades claras del color selecionado
        {
            // esto de las shades o shading s el mismo color pero en tonalidades mas claras y obscuras que pueden servr para sombrear
            // los tonos claros los obtengo disminuyendo la luminosidad del color y los oscuros aumentando la luminosidad, 
            // es por eso que obtengo el color en hsl 
            LightShades = new Color[4];
            HSL hsl = color.ToHsl();
            if(hsl.h == 0 && hsl.s == 0) // detyecto si el color es alguno tono de gris, es decir si se encuentra en la escala de grises para obtenerlos un poco diferente,pero la esencia del algoritmo es la misma
            {
                byte l_aux = 85;
                for(int i = 0; i < LightShades.Length; i++)
                {
                    hsl.l = l_aux;
                    LightShades[i] = hsl.ToRgb();
                    l_aux -= 10;
                }
            }
            else
            {
                byte l_aux = 95;
                for (int i = 0; i < LightShades.Length; i++)
                {
                    hsl.l = l_aux;
                    LightShades[i] = hsl.ToRgb();
                    l_aux -= 10;
                }
            }

            return LightShades;
        }

        public static Color[] GetDarkShades(Color color) // obtengo las shades oscuras del color selecionado
        {
            DarkShades = new Color[4];
            HSL hsl = color.ToHsl();
            if (hsl.h == 0 && hsl.s == 0)
            {
                byte l_aux = 5;
                for (int i = 0; i < DarkShades.Length; i++)
                {
                    hsl.l = l_aux;
                    DarkShades[i] = hsl.ToRgb();
                    l_aux += 10;
                }
            }
            else
            {
                byte l_aux = 10;
                for (int i = 0; i < DarkShades.Length; i++)
                {
                    hsl.l = l_aux;
                    DarkShades[i] = hsl.ToRgb();
                    l_aux += 10;
                }
            }

            return DarkShades;
        }

        public static Color getRandomColor() // obtengo un color aleatorio
        {
            /*
             * obtengo el color en hsl, para ayudarme un poco de que el color que se selecciona sea atractivo y no se vea feo
             * el algoritmo elige primero un valor para h de 0 a 360, despues para s de 35 a 100, y por ultimo de l de 20 a 85
             * lo hago entre esos rangos porque esos son los rangos en los cuales la saturacion del color es adeccuada y el color 
             * no se encuentra en la escala de grises o es muy laro o muy oscuro con esos valores de luminosidad
             * voy cambiando la semila de generacion de los valores con respecto al primer valor para que haya mas aleatoriedad 
             * pero no tanta y que el color no sea bonito, pero de esto ultimo la verdad no estoy muy segura, puede mejorar.
            */
            int h;
            byte s, l;
            Random rnd = new Random();
            h = rnd.Next(360);
            rnd = new Random(h*10);
            s = (byte)rnd.Next(35, 100);
            rnd = new Random(h*5);
            l = (byte)rnd.Next(20, 85);
            HSL hsl = HSL.FromHSL(h, s, l);
            Color rndColor = hsl.ToRgb();
            rndColor.A = 250;
            return rndColor;
        }

        static public Color GetPixelColor(int x, int y) // obtengo el color de un pixel de pantalla
        {
            // en este metodo hago uso de unas funciones de las librerias user32 y gdi32, las cuales 
            // me permiten acceder a informacion un poco mas profunda por asi decirlo de la computadora, dichas funciones reciben y regresan direciones de memoria 
            IntPtr hdc = GetDC(IntPtr.Zero); // primero obtengo una referencia al escritorio, o mas bien la direcion de memoria
            uint pixel = GetPixel(hdc, x, y); // aqui obtengo el pixel de la osision especificada del escritorio
            ReleaseDC(IntPtr.Zero, hdc); // capturo la pantalla o el escritorio
            Color color = Color.FromRgb((byte)(pixel & 0x000000FF), // obtengo el rgb del pixel y le aplico unas operaciones logicas para obtener el valor rgb 
                         Convert.ToByte((pixel & 0x0000FF00) >> 8),
                         Convert.ToByte((pixel & 0x00FF0000) >> 16));
            return color;
        }
    }

    class HSL // esta clase la ocupo para representar y manejar mas facilmente el color en hsl
    {
        private int H;
        private byte S, L;
        
        private HSL(int h, byte s, byte l)
        {
            H = h;
            S = s;
            L = l;
        }

        public static HSL FromHSL(int h, byte s, byte l) // establezco el color que representa la clase 
        {
            if (h < 0 || h > 360)
                h = 0;
            if (s > 100)
                s = 0;
            if (l > 100)
                l = 0;

            HSL hsl = new HSL(h, s, l);
            return hsl;

        }

        public Color ToRgb() // paso de un color en hsl a uno en rgb, para estas transformaciones utilize los algoritmos de la especificacion de css3
        {
            double double_l = l / 100.0;
            double double_h = h / 360.0;
            double double_s = s / 100.0;
            double double_r, double_g, double_b, p, q, rh, bh, gh;
            byte r, g, b;

            if(double_s == 0)
            {
                double_r = double_l;
                double_g = double_l;
                double_b = double_l;
            }
            else
            {
                if (double_l <= 0.5)
                    q = double_l * (double_s + 1);
                else
                    q = double_l + double_s - double_l * double_s;

                p = double_l * 2 - q;

                rh = double_h + 0.33333;
                gh = double_h;
                bh = double_h - 0.33333;

                double_r = getRgb(p, q, rh);
                double_g = getRgb(p, q, gh);
                double_b = getRgb(p, q, bh);
            }

            r = Convert.ToByte(double_r * 255.0);
            g = Convert.ToByte(double_g * 255.0);
            b = Convert.ToByte(double_b * 255.0);
            Color color = Color.FromRgb(r, g, b);
            return color;
        }

        private static double getRgb(double q1, double q2, double hue)
        {
            if (hue < 0)
                hue += 1;
            if (hue > 1)
                hue -= 1;

            if ((hue * 6) < 1)
                return q1 + (q2 - q1) * hue * 6;
            else if ((hue * 2) < 1)
                return q2;
            else if ((hue * 3) < 2)
                return q1 + (q2 - q1) * (0.6666666667 - hue) * 6;
            else
                return q1;
        }

        public int h
        {
            get
            {
                return H;
            }
            set
            {
                if (value <= 360 && value > 0)
                    H = value;
            }
        }

        public byte s
        {
            get
            {
                return S;
            }
            set
            {
                if (DesignControl.IsPercent(value))
                    S = value;
            }
        }

        public byte l
        {
            get
            {
                return L;
            }
            set
            {
                if (DesignControl.IsPercent(value))
                    L = value;
            }
        }
    }
}
