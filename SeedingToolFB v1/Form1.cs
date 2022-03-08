using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

/**
 * Create by tientoan on 03/03/2022
 * fb.com/tientoan.2503
 */
namespace SeedingToolFB_v1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        // Chon danh sach via
        private void button1_Click(object sender, EventArgs e)
        {
            chooseFileAndInsertTb(tbListVia);
        }

        // Chon dach sach bai viet seeding
        private void button2_Click(object sender, EventArgs e)
        {
            chooseFileAndInsertTb(tbListSeeding);
        }

        // Choose file and insert to textbox
        private void chooseFileAndInsertTb(TextBox tb)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                tb.Text = openFileDialog.FileName;
            }
        }

        // Bat dau seeding
        private void button3_Click(object sender, EventArgs e)
        {
            //if (tbListVia.Text.Trim().Length > 0 && tbListSeeding.Text.Trim().Length > 0)
            //{
            // Lay ra list cac via
            List<ViaModel> listVia = getViaList(tbListVia.Text);
            // Lấy ra list bài viết cần seeding
            List<string> listPost = getListSeeding(tbListSeeding.Text);

            if (listVia.Count > 0 && listPost.Count > 0)
            {

                foreach (ViaModel via in listVia)
                {
                    // TODO tao nhieu thread
                    Thread thread = new Thread(() => {
                    IWebDriver chrome = createWebDriver();

                    // login fb
                    login(chrome, via.userName, via.password);

                    // truy cập bài viết
                    while (!isLoginSuccess(chrome)) { }

                    // vào từng bài viết có trong list để comment
                    foreach (string post in listPost)
                    {
                        openNewTab(chrome);
                        goToSeedingPost(chrome, post);
                        commentSeeding(chrome, "Chúc ngày mới vui vẻ!");
                    }

                    });

                    thread.Start();

                }
            }
            else
            {
                MessageBox.Show("Danh sách trống! Vui lòng kiểm tra lại");
            }
            //}
            //else
            //{
            //    MessageBox.Show("Chọn đường dẫn đầy đủ để bắt đầu!");
            //}
        }

        // mở tab mới
        private void openNewTab(IWebDriver driver)
        {
            driver.FindElement(By.CssSelector("body")).SendKeys(OpenQA.Selenium.Keys.Control + "t");
            driver.SwitchTo().Window(driver.WindowHandles.Last());
        }

        // Den bai viet seeding
        private void goToSeedingPost(IWebDriver driver, string postLink)
        {
            navigate(driver, postLink);
        }

        // Comment bai viet
        private void commentSeeding(IWebDriver driver, string comment)
        {
            while (getCommentInput(driver) == null) { }
            var input = getCommentInput(driver);
            input.SendKeys(comment);
            input.SendKeys(OpenQA.Selenium.Keys.Enter);
        }

        // Lấy ra element bình luận
        private IWebElement getCommentInput(IWebDriver driver)
        {
            try
            {
                return driver.FindElement(By.XPath("//div[@aria-label='Viết bình luận']/p"));
            }
            catch (Exception e)
            {
                return null;
            }
        }

        // Khoi tao web driver
        private IWebDriver createWebDriver()
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArguments("--disable-notifications"); // tat thong bao alert
            return new ChromeDriver(options);
        }

        // Điền email
        private void enterUsername(IWebDriver driver, string email)
        {
            var input = driver.FindElement(By.Id("email"));
            input.SendKeys(email);
        }

        // Điền mat khau
        private void enterPassword(IWebDriver driver, string password)
        {
            var input = driver.FindElement(By.Id("pass"));
            input.SendKeys(password);
        }

        // click dang nhap
        private void clickLogin(IWebDriver driver)
        {
            var button = driver.FindElement(By.Name("login"));
            button.Click();
        }

       
        // login fb
        private void login(IWebDriver driver, string username, string password)
        {
            try
            {
                // truy cap fb
                navigate(driver, "http://www.fb.com");

                // Nhap thong tin tai khoan, mat khau
                enterUsername(driver, username);
                enterPassword(driver, password);

                // click dang nhap
                clickLogin(driver);
            } catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }


        }

        // check login thanh cong
        private bool isLoginSuccess(IWebDriver driver)
        {
            try
            {
                var f = driver.FindElement(By.XPath("//a[@aria-label='Trang chủ']"));
                return true;
            } catch (Exception ex)
            {
                return false;
            }
        }

        // Lấy ra danh sách bài viết cần seeding
        private List<string> getListSeeding(string filePath)
        {
            List<string> list = new List<string>();
            string[] lines = File.ReadAllLines(/*filePath*/@"C:\Users\Tien Toan\Documents\post.txt");
            foreach (var line in lines)
            {
                list.Add(line);
            }
            return list;
        }

        // Lay ra danh sach tai khoan via
        private List<ViaModel> getViaList(String filePath)
        {
            List<ViaModel> listVia = new List<ViaModel>();
            string[] lines = File.ReadAllLines(/*filePath*/@"C:\Users\Tien Toan\Documents\listvia.txt");
            ViaModel via = new ViaModel();

            List<int> incorrectFormatList = null;
            int count = 0;
            bool hasIncorrectFormat = false;

            foreach (string line in lines)
            {
                count++;
                string username = "";
                string password = "";

                // phải đúng format lấy ra được username và tài khoản mới tiếp tục
                try
                {
                    username = line.Trim().Substring(0, line.IndexOf(';'));
                    password = line.Trim().Substring(line.IndexOf(';') + 1);
                }
                catch (Exception e)
                {
                    hasIncorrectFormat = true;

                    if (incorrectFormatList == null)
                    {
                        incorrectFormatList = new List<int>();
                    }
                    incorrectFormatList.Add(count);
                }

                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                {
                    via.userName = username;
                    via.password = password;
                    listVia.Add(via);
                }
            }

            // thông báo các dòng sai format
            if (hasIncorrectFormat)
            {
                string toast = "";
                foreach (int i in incorrectFormatList)
                {
                    toast += $"{i}; ";
                }
                MessageBox.Show($"Các tài khoản điền sai format: {toast}");
            }
            return listVia;
        }

        // web navigate with try catch
        private void navigate(IWebDriver driver, string url)
        {
            try
            {
                driver.Url = url;
                driver.Navigate();
            } catch (Exception e)
            {
                MessageBox.Show(e.Message + "");
            }
        }
    }
}
