using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;
// Elaborado por Arely M.

// Desde aqui controlo el diseño de la aplicacion o sus controles visuales
namespace Slash_Design
{
    class DesignControl
    {
        private static MainWindow mainWindow { get; set;}
        private bool Theme;
        public bool IsAlpha;

        public DesignControl(MainWindow window)
        {
            mainWindow = window;
            theme = false;
            IsAlpha = false;
        }

        public void ShowAbout()
        {
            mainWindow.content_about.Visibility = Visibility.Visible;
            mainWindow.content_shading.Visibility = Visibility.Hidden;
        }

        public void ShowShadingMenu()
        {
            mainWindow.content_about.Visibility = Visibility.Hidden;
            mainWindow.content_shading.Visibility = Visibility.Visible;
            setShades(ColorTools.ActualColor);
        }

        public void isAlpha(bool enable) // hago unos ajustes al habilitar o deshabilitar la funcion alfa, como desactivar los controles relacionados a eso
        {
            IsAlpha = enable;
            if (enable)
            {
                mainWindow.tgb_alpha.ToolTip = "Disable Alpha Slider";
                mainWindow.txt_hex.MaxLength = 9;
                mainWindow.sld_alpha.IsEnabled = true;
                mainWindow.txt_alpha.IsEnabled = true;
            }
            else
            {
                mainWindow.tgb_alpha.ToolTip = "Enable Alpha Slider";
                mainWindow.txt_hex.MaxLength = 7;
                mainWindow.sld_alpha.IsEnabled = false;
                mainWindow.txt_alpha.IsEnabled = false;
            }
        }

        public void PickerColor(bool enable)
        {
            if (enable)
            {
                Mouse.OverrideCursor = Cursors.Cross; // cambio el cursor en toda la aplicacion
                mainWindow.tgb_alpha.IsChecked = false;
                // obtengo la ´posision de la ventana en ese momento
                double top = mainWindow.Top;
                double left = mainWindow.Left;
                // muevo la grid principal a la posision donde estabala ventana
                mainWindow.tt.X = left - 798;
                mainWindow.tt.Y = top - 323;
                // maximizo la ventana y la hago transparente para cambiar el cursor en toda la computadora por un momento y para que parezca que 
                // no estorbe al usuario se hace transparente lo que permite tambien poder elegir el color de un pixel de la pantalla
                mainWindow.WindowState = WindowState.Maximized;
                mainWindow.aux.IsEnabled = false; // deshabilito todos los controles de la aplicacion
                // me suscribo o me pongo en escucha de los eventos de click en la ventana principal que los voy a ocupar para obtener el color del pixe de 
                // pantala o salir de este modo
                mainWindow.MouseLeftButtonDown += new MouseButtonEventHandler(mainWindow.OnClick_Window); 
                mainWindow.MouseRightButtonDown += new MouseButtonEventHandler(mainWindow.OnClick_Window);
            }
            else
            { // regreso toda la ap al estado normal porque se deshabilito el modo de seleccionar el color de un pixel
                Mouse.OverrideCursor = Application.Current.MainWindow.Cursor;
                mainWindow.Top = mainWindow.tt.Y + 323;
                mainWindow.Left = mainWindow.tt.X + 798;
                mainWindow.tt.X = 0;
                mainWindow.tt.Y = 0;
                mainWindow.WindowState = WindowState.Normal;
                mainWindow.aux.IsEnabled = true;
                mainWindow.MouseLeftButtonDown -= new MouseButtonEventHandler(mainWindow.OnClick_Window);
                mainWindow.MouseRightButtonDown += new MouseButtonEventHandler(mainWindow.OnClick_Window);
            }
        }
        // Unas comprobaciones que utilizare para comprobar que el texto ingresado es correcto
        public static bool TextIsNumeric(string input)
        {
            return input.All(c => Char.IsDigit(c) || Char.IsControl(c));
        }

        public static bool IsHex(string input)
        {
            return input.All(c => TextIsNumeric(c.ToString()) || c == '#' || IsHexLetter(c.ToString()));
        }

        private static bool IsHexLetter(string input)
        {
            input = input.ToUpper();
            return input.All(c => c == 'A' || c == 'B' || c == 'C' || c == 'D' || c == 'F');
        }

        public static bool IsByte(int number)
        {
            return number <= 255;
        }

        public static bool IsPercent(int number)
        {
            return number <= 100 && number > 0;
        }
        // terminan las comprobaciones

        public void setColor(Color color) // cambio el color que se muestra al usuario
        {
            ColorTools.LastColor = ((SolidColorBrush)mainWindow.box_color.Fill).Color;
            ColorTools.ActualColor = color;
            mainWindow.box_color.Fill = new SolidColorBrush(color);
            mainWindow.sld_red.Value = color.R;
            mainWindow.sld_green.Value = color.G;
            mainWindow.sld_blue.Value = color.B;

            if (IsAlpha)
            {
                mainWindow.txt_hex.Text = color.ToHex(true);
                mainWindow.sld_alpha.Value = Math.Round(color.A / 2.5);
            }
            else
            {
                mainWindow.txt_hex.Text = color.ToHex(false);
            }
            // obtengo la luminosidad que se percibe del color selecionado para elegir el color del texto (blanco o negro)
            // segun se vea mejor con respecto al colr que eligio el usuario
            double luminance  = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255;
            if (luminance > 0.5 || color.A <= 102)
                mainWindow.txt_hex.Foreground = Application.Current.Resources["PrimaryHueLightForegroundBrush"] as SolidColorBrush;
            else
                mainWindow.txt_hex.Foreground = Application.Current.Resources["PrimaryHueDarkForegroundBrush"] as SolidColorBrush;
        }

        public Color getColor() // obtengo el color que tienen los controles de la aplicacion 
        {
            // los controles principales de la aplicacion que es de donde tomo el color es de los sliders
            Color color;
            if (IsAlpha)
            {
                color = Color.FromArgb(Convert.ToByte(mainWindow.sld_alpha.Value * 2.5), (byte)mainWindow.sld_red.Value, (byte)mainWindow.sld_green.Value, (byte)mainWindow.sld_blue.Value);
            }

            else
            {
                color = Color.FromRgb((byte)mainWindow.sld_red.Value, (byte)mainWindow.sld_green.Value, (byte)mainWindow.sld_blue.Value);
            }
            return color;
        }

        public bool theme // el tema claro u osbsuro de la aplicacion segun lo eliga el usuario
        {
            get
            {
                return Theme;
            }
            set
            {
                Theme = value;
                new PaletteHelper().SetLightDark(Theme);
            }
        }

        public void setShades(Color color) // pongo los cuadros donde se muestran los shades del color que selecciono el usuario 
        {
            Color[] LightShades = ColorTools.GetLightShades(color);
            Color[] DarkShades = ColorTools.GetDarkShades(color);

            mainWindow.sample_9.Fill = new SolidColorBrush(LightShades[0]);
            mainWindow.sample_9.ToolTip = LightShades[0].ToHex(false);

            mainWindow.sample_8.Fill = new SolidColorBrush(LightShades[1]);
            mainWindow.sample_8.ToolTip = LightShades[1].ToHex(false);

            mainWindow.sample_7.Fill = new SolidColorBrush(LightShades[2]);
            mainWindow.sample_7.ToolTip = LightShades[2].ToHex(false);

            mainWindow.sample_6.Fill = new SolidColorBrush(LightShades[3]);
            mainWindow.sample_6.ToolTip = LightShades[3].ToHex(false);


            mainWindow.sample_1.Fill = new SolidColorBrush(color);
            mainWindow.sample_1.ToolTip = color.ToHex(false);

            mainWindow.sample_5.Fill = new SolidColorBrush(DarkShades[0]);
            mainWindow.sample_5.ToolTip = DarkShades[0].ToHex(false);

            mainWindow.sample_4.Fill = new SolidColorBrush(DarkShades[1]);
            mainWindow.sample_4.ToolTip = DarkShades[1].ToHex(false);

            mainWindow.sample_3.Fill = new SolidColorBrush(DarkShades[2]);
            mainWindow.sample_3.ToolTip = DarkShades[2].ToHex(false);

            mainWindow.sample_2.Fill = new SolidColorBrush(DarkShades[3]);
            mainWindow.sample_2.ToolTip = DarkShades[3].ToHex(false);

        }
    }
}
