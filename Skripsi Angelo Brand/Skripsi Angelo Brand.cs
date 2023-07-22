using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV.GPU;
using Emgu.CV.UI;

namespace Skripsi_Angelo_Brand
{
    // PROGRAM SKRIPSI DETEKSI DAN PENGENALAN WAJAH MENGGUNAKAN METODE HAAR CASCADE DAN EIGENFACE
    // NAMA : ANGELO IANSON BRAND
    // NIM  : 1806080030

    public partial class Skripsi_Angelo_Brand : Form
    {
        // Variabel Font 
        MCvFont font = new MCvFont(FONT.CV_FONT_HERSHEY_TRIPLEX, 0.6d, 0.6d);

        // Variabel Library Haar-like Features
        HaarCascade deteksiWajah;
        HaarCascade deteksiMata;
        HaarCascade deteksiHidung;
        HaarCascade deteksiMulut;

        // Variabel Kamera
        Capture kamera;

        // List Wajah
        Image<Bgr, Byte> FrameWajah;
        
        Image<Gray, byte> hasilWajah;
        Image<Bgr, byte> hasilMata;
        Image<Bgr, byte> hasilHidung;
        Image<Bgr, byte> hasilMulut;
        Image<Bgr, byte> hasilWajahWarna;

        Image<Gray, byte> wajahTrain = null;
        Image<Gray, byte> grayscaleWajah = null;

        // List 
        List<Image<Gray, byte>> trainingImages = new List<Image<Gray, byte>>();

        List<string> labels = new List<string>();
        List<string> users = new List<string>();

        int hitung, jumlahWajah, t, centang;
        string name, names = null;

        public Skripsi_Angelo_Brand()
        {
            InitializeComponent();

            // Jika Belum Ada Direktori Wajah, Maka Buat Foldernya
            if (!Directory.Exists(Application.StartupPath + "/Wajah/"))
            {
                Directory.CreateDirectory(Application.StartupPath + "/Wajah/");
            }
            
            deteksiWajah = new HaarCascade("haarcascade_wajah.xml");    // Load file haar.xml untuk mendeteksi wajah
            deteksiMata = new HaarCascade("haarcascade_mata.xml");      // Load file haar.xml untuk mendeteksi mata
            deteksiHidung = new HaarCascade("haarcascade_hidung.xml");  // Load file haar.xml untuk mendeteksi hidung
            deteksiMulut = new HaarCascade("haarcascade_mulut.xml");    // Load file haar.xml untuk mendeteksi mulut
            
            // Cek Direktori Apakah Sudah Ada Wajah Tersimpan atau Belum
            try
            {
                string direktori = File.ReadAllText(Application.StartupPath + "/Wajah/Wajah.txt");
                string[] wajah = direktori.Split(',');

                jumlahWajah = Convert.ToInt16(wajah[0]);
                hitung = jumlahWajah;

                string muatWajah;

                for (int i = 1; i < jumlahWajah + 1; i++)
                {
                    muatWajah = "Wajah" + i + ".bmp";
                    trainingImages.Add(new Image<Gray, byte>(Application.StartupPath + "/Wajah/" + muatWajah));
                    labels.Add(wajah[i]);
                }
            }
            // Jika Tidak Ada Wajah yang Tersimpan Maka Tampilkan Pesan
            catch (Exception)
            {
                MessageBox.Show("Database wajah kosong! Silahkan tambahkan wajah baru.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // Tombol Mulai
        private void btMulai_Click(object sender, EventArgs e)
        {
            kamera = new Capture();
            kamera.QueryFrame();

            Application.Idle += new EventHandler(FrameProcedure);

            btMulai.Enabled = false;

            btSimpan.Enabled = true;
            btDatabase.Enabled = true;
            btRestart.Enabled = true;
            btDeteksi.Enabled = true;
            txtNamaSimpan.Focus();
            txtNamaSimpan.Enabled = true;
            btHapus.Enabled = true;
        }

        // Tombol Restart
        private void btRestart_Click(object sender, EventArgs e)
        {
            Application.Restart();
        }

        // Tombol Keluar
        private void btKeluar_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        // Tombol Simpan
        private void btSimpan_Click_1(object sender, EventArgs e)
        {
            // Jika Belum Memasukan Nama maka Tampilkan Pesan Error
            if (txtNamaSimpan.Text == "" || txtNamaSimpan.Text.Length < 2 || txtNamaSimpan.Text == string.Empty)
            {
                MessageBox.Show("Silahkan masukan nama terlebih dahulu!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                hitung += 1;

                // Mengubah Hasil Tangkapan Kamera ke Grayscale
                grayscaleWajah = kamera.QueryGrayFrame().Resize(320, 240, 
                INTER.CV_INTER_CUBIC);

                // Deteksi Wajah dengan Haar-like Fitur
                MCvAvgComp[][] DetectedFace = grayscaleWajah.DetectHaarCascade(deteksiWajah, 1.2, 10, 
                HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(20, 20));

                foreach (MCvAvgComp f in DetectedFace[0])
                {
                    wajahTrain = FrameWajah.Copy(f.rect).Convert<Gray, Byte>();
                    break;
                }

                // Resize Ukuran Menjadi 100 x 100 Piksel
                wajahTrain = hasilWajah.Resize(100, 100, INTER.CV_INTER_CUBIC);

                trainingImages.Add(wajahTrain);
                imgTraining.Image = hasilWajahWarna;
                imgMata.Image = null;
                imgHidung.Image = null;
                imgMulut.Image = null;

                labels.Add(txtNamaSimpan.Text);

                // Menyimpan Nama dan Wajah yang Sudah di-Resize ke Direktori Wajah
                File.WriteAllText(Application.StartupPath + "/Wajah/Wajah.txt", trainingImages.ToArray().Length.ToString() + ",");

                for (int i = 1; i < trainingImages.ToArray().Length + 1; i++)
                {
                    trainingImages.ToArray()[i - 1].Save(Application.StartupPath + "/Wajah/Wajah" + i + ".bmp");
                    File.AppendAllText(Application.StartupPath + "/Wajah/Wajah.txt", labels.ToArray()[i - 1] + ",");
                }

                // Menampilkan Pesan Ketika Wajah Berhasil Disimpan
                MessageBox.Show("Wajah " + txtNamaSimpan.Text + " berhasil ditambahkan!");
            }
        }

        private void FrameProcedure(object sender, EventArgs e)
        {
            users.Add("");
            txJumlahOrang.Text = "0";

            FrameWajah = kamera.QueryFrame().Resize(320, 240, INTER.CV_INTER_CUBIC);
            grayscaleWajah = FrameWajah.Convert<Gray, Byte>();

            // Haar-like Fitur untuk Mendeteksi Wajah, Mata, Hidung dan Mulut
            MCvAvgComp[][] faceDetectedShow = grayscaleWajah.DetectHaarCascade(deteksiWajah, 1.2, 10, 
            HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(20, 20));
            
            MCvAvgComp[][] eyeDetectedShow = grayscaleWajah.DetectHaarCascade(deteksiMata, 1.2, 10, 
            HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(15, 15));
            
            MCvAvgComp[][] noseDetectedShow = grayscaleWajah.DetectHaarCascade(deteksiHidung, 1.2, 10, 
            HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(15, 15));
            
            MCvAvgComp[][] mouthDetectedShow = grayscaleWajah.DetectHaarCascade(deteksiMulut, 1.5, 10, 
            HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(18, 18));

            // Menggambar Kotak Pada Daerah Mata
            foreach (MCvAvgComp mata in eyeDetectedShow[0])
            {
                hasilMata = FrameWajah.Copy(mata.rect).Convert<Bgr, Byte>().Resize(100, 100, INTER.CV_INTER_CUBIC);
                
                if (centang == 1)
                {
                    FrameWajah.Draw(mata.rect, new Bgr(Color.Green), 1);                    
                }
                else
                {
                    centang = 0;
                }
            }

            // Menggambar Kotak Pada Daerah Hidung
            foreach (MCvAvgComp hidung in noseDetectedShow[0])
            {
                hasilHidung = FrameWajah.Copy(hidung.rect).Convert<Bgr, Byte>().Resize(100, 100, INTER.CV_INTER_CUBIC);
                if (centang == 1)
                {
                    FrameWajah.Draw(hidung.rect, new Bgr(Color.Blue), 1);                    
                }
                else
                {
                    centang = 0;
                }
            }

            // Menggambar Kotak Pada Daerah Mulut
            foreach (MCvAvgComp mulut in mouthDetectedShow[0])
            {
                hasilMulut = FrameWajah.Copy(mulut.rect).Convert<Bgr, Byte>().Resize(100, 100, INTER.CV_INTER_CUBIC);
                if (centang == 1)
                {
                    FrameWajah.Draw(mulut.rect, new Bgr(Color.Yellow), 1);                    
                }
                else
                {
                    centang = 0;
                }
            }

            // Menggambar Kotak Pada Daerah Wajah
            foreach (MCvAvgComp wajah in faceDetectedShow[0])
            {
                t += 1;

                // Tampilkan Kotak pada Wajah yang Terdeteksi
                hasilWajah = FrameWajah.Copy(wajah.rect).Convert<Gray, Byte>().Resize(100, 100, INTER.CV_INTER_CUBIC);
                hasilWajahWarna = FrameWajah.Copy(wajah.rect).Convert<Bgr, Byte>().Resize(100, 100, INTER.CV_INTER_CUBIC);

                if (centang == 1)
                {
                    FrameWajah.Draw(wajah.rect, new Bgr(Color.Aqua), 1);                    
                }
                else
                {
                    centang = 0;
                }

                // Proses Identifikasi dengan Metode Eigenface
                if (trainingImages.ToArray().Length > 0)
                {
                    float eucDistance = 0;
                    float eigenThreshold = 2000;
                    MCvTermCriteria termCriterias = new MCvTermCriteria(hitung, 0.001);
                    EigenObjectRecognizer recognizer = new EigenObjectRecognizer
                    (trainingImages.ToArray(),labels.ToArray(), 
                    4000,
                    ref termCriterias);

                    for (int i = 0; i < trainingImages.ToArray().Length; i++)
                    {
                        eucDistance = recognizer.GetEigenDistances(hasilWajah)[i];
                        if (eucDistance < eigenThreshold)
                        {
                            name = recognizer.Recognize(hasilWajah);
                            break;
                        }
                        else
                        {
                            name = "Tidak Kenal";
                        }
                    }
                                        
                    //FrameWajah.Draw(eucDistance.ToString(), ref font, new Point(wajah.rect.X - 2, wajah.rect.Y - 2), new Bgr(Color.Red));
                }
                else
                {
                    name = "Tidak Kenal";
                }

                users[t - 1] = name;
                users.Add("");

                // Menampilkan Jumlah Wajah yang Terdeteksi
                txJumlahOrang.Text = faceDetectedShow[0].Length.ToString();
                users.Add("");
            }

            t = 0;

            // Menampilkan Mama Wajah yang Terdeteksi
            for (int nnn = 0; nnn < faceDetectedShow[0].Length; nnn++)
            {
                names = names + users[nnn] + ", ";
            }

            // Menampilkan Hasil Identifikasi
            cameraBox.Image = FrameWajah;
            txNamaKenal.Text = names;
            names = "";
            users.Clear();
        }

        // Menampilkan Informasi Ketika Menekan Tombol Deteksi
        private void btDeteksi_Click(object sender, EventArgs e)
        {
            if (txNamaKenal.Text.Length == 0 && txJumlahOrang.Text == "0")
            {
                txNamaKenal.Text = "Tidak Ada Wajah";
                imgTraining.Image = null;
            }
            else
            {
                imgTraining.Image = hasilWajahWarna;
                imgMata.Image = hasilMata;
                imgHidung.Image = hasilHidung;
                imgMulut.Image = hasilMulut;
            }

            MessageBox.Show("\nNama : " + txNamaKenal.Text + "\nJumlah Wajah : " + txJumlahOrang.Text, "Informasi Deteksi", MessageBoxButtons.OK);
        }

        // Membersihkan Semua Picturebox
        private void btHapus_Click(object sender, EventArgs e)
        {
            if (imgTraining.Image == null && imgMata.Image == null && imgHidung.Image == null && imgMulut.Image == null)
            {
                MessageBox.Show("Picture Box Sudah Bersih!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                imgTraining.Image = null;
                imgMata.Image = null;
                imgHidung.Image = null;
                imgMulut.Image = null;
            }
        }

        // Membuka Folder Database
        private void btDatabase_Click(object sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = Application.StartupPath + "/Wajah/",
                UseShellExecute = true,
                Verb = "open"
            });
        }

        // Menampilkan Kotak Deteksi / Bounding Box
        private void checkBox1_CheckedChanged_1(object sender, EventArgs e)
        {
            if (centang == 0)
            {
                centang = 1;
            }
            else
            {
                centang = 0;
            }
        }
    }
}