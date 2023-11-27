using System;
using System.Text;
using System.Windows.Forms;
using System.Management;
using Microsoft.Win32;
using System.Linq;

namespace keyGen
{
    public partial class Form1 : Form
    {
        private static Random rand = new Random();
        string MBid = string.Empty;
        bool bandQuery = false;
        public Form1()
        {
            InitializeComponent();
        }

        private void btnGenerar_Click(object sender, EventArgs e)
        {
            ManagementObjectSearcher mos = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard");
            ManagementObjectCollection moc = mos.Get();
            foreach (ManagementObject mo in moc)
            {
                bandQuery = true;
                MBid = (string)mo["SerialNumber"];
                Console.WriteLine(MBid);
            }

            if (bandQuery == true)
            {
                Generar(MBid);
            }
            
        }

        private void Generar(string mbid)
        {
            int valorLetra = 0, comodin = 0, separador = 0;
            byte[] posicion;
            char[] c, mb;
            string[] fchhex = FchToHex(),
                     clvFch;
            string clave = string.Empty;
            const string car = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            c = car.ToCharArray();
            mb = mbid.ToCharArray();

            for (int i=0; i<mbid.Length; i++)
            {
                posicion = Encoding.ASCII.GetBytes(mbid.Substring(i, 1));
                valorLetra = posicion[0];
                Console.WriteLine(valorLetra);
                for (int j = 0; j <= valorLetra; j++)
                {
                    if (comodin == 26)
                        comodin = 0;
                    if (separador == 5)
                    {
                        clave = clave + "-";
                        separador = 0;
                    }
                    if (j == valorLetra)
                    {
                        clave = clave + c[comodin];
                        separador++;
                    }
                    comodin++;
                }
            }

            comodin = 25;

            for (int i=0; i<mbid.Length; i++)
            {
                posicion = Encoding.ASCII.GetBytes(mbid.Substring(i, 1));
                valorLetra = posicion[0];
                Console.WriteLine(valorLetra);
                for (int j = 0; j <= valorLetra; j++)
                {
                    if (comodin == 0)
                        comodin = 25;
                    if (separador == 5)
                    {
                        clave = clave + "-";
                        separador = 0;
                    }
                    if (j == valorLetra)
                    {
                        clave = clave + c[comodin];
                        separador++;
                    }
                    comodin--;
                }
            }

            clvFch = clave.Split('-');
            clave = clvFch[0] + "-" + clvFch[1] + "-" + clvFch[2] + "-" + fchhex[0] + "-" +clvFch[3];

            //Se crea el registro Cotta y se le almacena la Llave escrita por el usuario.
            Registry.CurrentUser.CreateSubKey("Software\\Win", RegistryKeyPermissionCheck.Default);
            Registry.CurrentUser.CreateSubKey("Software\\Win").SetValue("Terra", clave);
            Registry.CurrentUser.CreateSubKey("Software\\Win").SetValue("TerraDate", fchhex[0]);
            Registry.CurrentUser.CreateSubKey("Software\\Win").SetValue("TerraCert", "False");
            Registry.CurrentUser.CreateSubKey("Software\\Win").Close();

            MessageBox.Show(clave);
        }

        private string[] FchToHex()
        {
            string[] fchtohex = new string[2];
            //Variables para la fecha en la clave
            string fchstrdd = String.Empty,
                   fchstrmm = String.Empty,
                   fchstraa = String.Empty,
                   fch = String.Empty;
            int fchintdd = 0,
                fchintmm = 0,
                fchintaa = 0;
            DateTime dt1 = DateTime.Now;

            //Fecha para el keyGen
            //Extraemos la fecha diseccionada
            fchstrdd = dt1.ToString("dd");
            fchstrmm = dt1.ToString("MM");
            fchstraa = dt1.ToString("yy");


            //Convertimos cada seccion de la fecha en int
            //Comprobamos que el valor en decimal, si es menor que 15 se le agrega un 0 al valor hexadecimal
            //Se le suma un numero de 14 al 16 para agregar un factor de aletoriedad
            fchintdd = Convert.ToInt32(fchstrdd) + 16;
            if (fchintdd <= 15)
                fchstrdd = "0" + fchintdd.ToString("X");
            else
                fchstrdd = fchintdd.ToString("X");

            fchintmm = Convert.ToInt32(fchstrmm) + 15;
            if (fchintmm <= 15)
                fchstrmm = "0" + fchintmm.ToString("X");
            else
                fchstrmm = fchintmm.ToString("X");

            fchintaa = 1 + Convert.ToInt32(fchstraa) + 14;
            if (fchintaa <= 15)
                fchstraa = "0" + fchintaa.ToString("X");
            else
                fchstraa = fchintaa.ToString("X");

            //La segundo seccion de la Clave se le asigna la fecha en hexadecimal
            fch = fchstrdd + fchstrmm + fchstraa;
            fchtohex[0] = fch;

            return fchtohex;
        }
    }
}
