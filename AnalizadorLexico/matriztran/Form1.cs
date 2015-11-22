using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.OleDb;
using System.IO;
using System.Diagnostics;

namespace matriztran
{
    public partial class Form1 : Form
    {
        List<string> columnas = new List<string>();

        public Form1()
        {
            InitializeComponent();
            RecargarMatriz();
            LeerColumnas();
        }

        private void LeerColumnas()
        {
            for (int i = 1; i < DtSet.Tables[0].Columns.Count-1; i++)
            {
                string aux = DtSet.Tables[0].Columns[i].Caption.ToString();

                if (aux == "PUNTO")
                {
                    columnas.Add(".");
                }
                else if (aux == "NOT")
                {
                    columnas.Add("!");
                }
                else if (aux == "COR_A")
                {
                    columnas.Add("[");
                }
                else if (aux == "COR_C")
                {
                    columnas.Add("]");
                }
                else if (aux == " ")
                {
                    columnas.Add("⇢");
                }
                else if (aux.Contains("1") && aux != "1")
                {
                    aux = aux.Substring(0,1);
                    columnas.Add(aux);
                }
                else
                {
                    columnas.Add(aux);
                }
            }
        }

        DataSet DtSet;
        private void RecargarMatriz()
        {
            OleDbConnection MyConnection;
            OleDbDataAdapter MyCommand;
            MyConnection = new OleDbConnection(@"provider=Microsoft.ACE.OLEDB.12.0;Data Source='C:/Datos/lenguaje.xlsx';Extended Properties=Excel 12.0;");
            MyCommand = new OleDbDataAdapter("select * from [Hoja1$]", MyConnection);

            MyCommand.TableMappings.Add("Table", "Table");
            DtSet = new DataSet();
            DtSet.CaseSensitive = true;
            
            MyCommand.Fill(DtSet);
            MyConnection.Close();
        }

        string id;
        int indice = 0;
        int Estado = 0;
        bool DEL = false;
        int ContIds = 0;
        private void btnValidar_Click(object sender, EventArgs e)
        {
            RecargarMatriz();
            id = txtId.Text;
            id = id.Replace(Environment.NewLine, " ");
            indice = 0;
            Estado = 0;
            DEL = false;
            txtTokens.Text = "";
            ContIds = 0;
            bool BanCadena = false;
            bool[] BanCom = { false, false};
            ///////////////
            try
            {
                while (indice <= id.Length)
                {
                    char Actual = '␀';

                    if (indice == id.Length)
                        DEL = true;
                    else
                    {
                        Actual = id[indice];
                        if (Actual == '"' && !BanCadena && (!BanCom[0] && !BanCom[1]))
                            BanCadena = true;
                        else if (Actual == '"' && BanCadena)
                            BanCadena = false;

                        if (Actual == '/')
                        {
                            if (!BanCom[0] && !BanCom[1]) //0 0
                                BanCom[0] = true;
                            else if (BanCom[0] && !BanCom[1])//1 0
                                BanCom[1] = true;
                            else if (BanCom[0] && BanCom[1])//1 1
                                BanCom[0] = false;
                            else if (!BanCom[0] && BanCom[1])//0 1
                                BanCom[1] = false;
                        }

                        if (Actual == ' ' && (BanCadena || (BanCom[0] && BanCom[1])))
                            Actual = '⇢';
                        else if (Actual == ' ' && !BanCadena)
                            DEL = true;
                    }
                    bool bandera = false;

                    for (int i = 0; i < columnas.Count; i++)
                    {
                        string celda = DtSet.Tables[0].Rows[Estado][i + 1].ToString();
                        if (DEL)
                        {
                            if (columnas[i] == "DEL")
                            {
                                Estado = int.Parse(celda);
                                Estado--;
                                celda = DtSet.Tables[0].Rows[Estado][i + 1].ToString();

                                if (celda.Contains("ACEPTA"))
                                {
                                    string token = DtSet.Tables[0].Rows[Estado][i + 2].ToString();
                                    txtTokens.Text += token;
                                    if (token == "ID")
                                        txtTokens.Text += ++ContIds;
                                    txtTokens.Text += " ";
                                    bandera = true;
                                    Estado = 0;
                                    DEL = false;
                                }
                                else if (celda != "ERROR" && celda != "")
                                {
                                    Estado = int.Parse(celda);
                                    Estado--;
                                    bandera = true;
                                }
                            }
                        }
                        else if (Actual.ToString() == columnas[i])
                        {
                            MessageBox.Show(Actual.ToString() + " " + celda);
                            if (celda != "ERROR" && celda != "")
                            {
                                Estado = int.Parse(celda);
                                Estado--;
                                bandera = true;
                            }
                            else if (celda == "ERROR")
                            {
                                throw new Exception("Error de léxico");
                                
                            }
                        }
                    }
                    if (!bandera)
                    {
                        throw new Exception("Error de léxico");
                    }
                    indice++;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            Acomodar();
        }

        private bool Contains(char c, char[] arr)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                if (c == arr[i])
                    return true;
            }
            return false;
        }

        private void Acomodar()
        {
            string fuente = txtId.Text;
            string destino = txtTokens.Text;
            fuente = fuente.Replace("\r\n", "▾");
            List<int> espacios = new List<int>();

            int cont = 0;
            foreach (char x in fuente)
            {
                if(x == ' ')
                    cont++;
                if (x == '▾')
                {
                    cont++;
                    espacios.Add(cont);
                }
            }

            cont = 0;
            txtTokens.Text = "";
            for (int i = 0; i < destino.Length; i++)
            {
                if (destino[i] == ' ')
                {
                    cont++;
                    if (espacios.Contains(cont))
                        txtTokens.Text += Environment.NewLine;
                    else
                        txtTokens.Text += destino[i];
                }
                else
                    txtTokens.Text += destino[i];
            }

            string Ruta = @"C:\datos\";
            if (!Directory.Exists(Ruta))
                Directory.CreateDirectory(Ruta);
            string archivo = DateTime.Now + ".txt";
            archivo = archivo.Replace('/', '.');
            archivo = archivo.Replace(':','.');
            Ruta += archivo;
            //FileStream FS = new FileStream(Ruta, FileMode.Create,FileAccess.Write);
            StreamWriter SW = new StreamWriter(Ruta, true, Encoding.UTF8);
            foreach (char c in txtTokens.Text)
            {
                SW.Write(c);
            }
            SW.Close();
            Process.Start(Ruta);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            int width = (this.Size.Width - 46) / 2;
            int height = (this.Size.Height - 92);
            this.txtId.Size = new Size(width, height);
            this.txtTokens.Size = new Size(width, height);
            this.txtTokens.Location = new Point(width + 20, 41);
            btnValidar.Location = new Point((this.Size.Width - 90) / 2, 10);
        }
    }
}
