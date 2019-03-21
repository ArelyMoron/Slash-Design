using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

// Elaborado por Arely M.
/*
 * Version 2018.1 
 ** en la proxima actualizacion pienso incluir una ventana para guardar paletas de colores. **
*/
namespace Slash_Design
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DesignControl design; // esta clase la ocupo para aceder a los controles de diseño de la aplicacion

        public MainWindow()
        {
            InitializeComponent();

            design = new DesignControl(this);
            design.setColor(Color.FromArgb(255, 137, 200, 100)); // pongo un color por defecto para que se muestre 
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) // funcion de arrastrar y mover cuando se de click al borde de la ventana
        {
            DragMove();
        }

        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btn_minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        public void OnChangeColor(object sender, RoutedPropertyChangedEventArgs<double> e) // cambio el color que se muestra en el cuadro 
        {
            design.setColor(design.getColor());
        }

        private void OnTextChange(object sender, TextCompositionEventArgs e) // comprobaciones antes de que se escriba e impedir entradas incorrectas
        {
            TextBox textBox = sender as TextBox; // obtengo el textbox que produjo el evento
            string ActualText = textBox.Text + e.Text;
            if (textBox.Name == txt_hex.Name) // compruebo el texto que se escribe en el cuadro de texto de hex
            {
                if (DesignControl.IsHex(e.Text))
                {
                    if (e.Text == "#" && ActualText.Length > 1)
                        e.Handled = true;
                }
                else
                {
                    e.Handled = true;
                }
            }
            else // compruebo el texto en los otros cuadros de texto
            {
                if (DesignControl.TextIsNumeric(e.Text))
                {
                    int number = Convert.ToInt32(ActualText);
                    if (textBox.Name != txt_alpha.Name)
                        e.Handled = !DesignControl.IsByte(number);
                    else
                        e.Handled = !DesignControl.IsPercent(number);
                }
                else
                    e.Handled = true;
            }
        }


        private void OnCheckedChange(object sender, RoutedEventArgs e) // habilita y deshabilita la funcion de alpha 
        {
            design.isAlpha((bool)tgb_alpha.IsChecked);
            OnChangeColor(this, new RoutedPropertyChangedEventArgs<double>(0, 0));
        }

        private void btn_about_Click(object sender, RoutedEventArgs e)
        {
            design.ShowAbout(); // abro la seccion de about me
        }

        private void btn_shadingColor_Click(object sender, RoutedEventArgs e)
        {
            design.ShowShadingMenu(); // abro la sectablas de colores (shading)
        }

        private void OnThemeChange(object sender, RoutedEventArgs e)
        {
            design.theme = !design.theme;
        }

        private void OnTextChange(object sender, TextChangedEventArgs e) // hago mas comprobaciones al ingresar texto e impido ingresar texto incorrecto
        {
            design = new DesignControl(this); // vuelvo a iniciar la variable design porque cuando se produce el evento por primera vez que 
            // es al cambiar el texto en xaml aun no se ha iniciado en esta clase
            TextBox textBox = sender as TextBox;
            if (textBox.Name == "txt_hex")
            {
                if (DesignControl.IsHex(textBox.Text))
                {
                    if (textBox.Text.Length == 9 && design.IsAlpha)
                    {
                        design.setColor(ColorTools.FromHex(textBox.Text));
                    }

                    else if (textBox.Text.Length == 7 && !design.IsAlpha)
                    {
                        design.setColor(ColorTools.FromHex(textBox.Text));
                    }
                }
                else
                {
                    txt_hex.Text = ColorTools.LastColor.ToHex(true);
                }
            }

            else if (textBox.Name != "txt_alpha")
            {
                if (!DesignControl.TextIsNumeric(textBox.Text) || string.IsNullOrWhiteSpace(textBox.Text))
                {
                    textBox.Text = "0";
                }
            }

            else
            {
                if (DesignControl.TextIsNumeric(textBox.Text) && !string.IsNullOrWhiteSpace(textBox.Text))
                {
                    int number = Convert.ToInt32(textBox.Text);
                    if (!DesignControl.IsPercent(number))
                    {
                        textBox.Text = "0";
                    }
                }
                else
                    textBox.Text = "0";

            }

        }

        private void OnClick_Sample(object sender, MouseButtonEventArgs e) // cuando doy click alas muestras de shading de los colores
        {
            System.Windows.Shapes.Rectangle sample = sender as System.Windows.Shapes.Rectangle;
            Clipboard.SetText(sample.ToolTip.ToString());
            snackbar.MessageQueue.Enqueue("Hex copied " + sample.ToolTip.ToString(), true);
        }

        private void btn_randomColor_Click(object sender, RoutedEventArgs e)
        {
            Color randomColor = ColorTools.getRandomColor();
            design.setColor(randomColor);
        }

        private void btn_vcode_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/ArelyMoron"); // abro la pagina web de este repositorio que es donde esta el codigo
        }

        private void btn_pickColor_Click(object sender, RoutedEventArgs e)
        {
            design.PickerColor(true); // ajustes necesarios al activar la funcion de selecionar el color de un pixel de pantalla 
        }

        public void OnClick_Window(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) // obtengo que boton se presiono del mouse y hago aciones al respecto
            {
                Point point = Mouse.GetPosition(Application.Current.MainWindow);
                Color color = ColorTools.GetPixelColor((int)point.X, (int)point.Y);
                design.setColor(color);
            }
            else if(e.ChangedButton == MouseButton.Right)
            {
                design.PickerColor(false); // si se presiona el boton derecgo del mouse salgo del modo selecionar color
            }
        }
    }
}

