using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Linq;

namespace koval_yp_codec
{
    public partial class MainForm : Form
    {
        // Панели
        private Panel menuPanel;
        private Panel encryptPanel;
        private Panel signaturePanel;
        private Panel historyPanel;
        private Panel logPanel;

        // Элементы главного меню
        private Button btnEncryptMenu, btnSignatureMenu, btnHistoryMenu, btnLogMenu;
        private ToolTip toolTip;

        // Элементы шифрования
        private ComboBox cmbCipher;
        private NumericUpDown nudShift;
        private TextBox txtInput, txtOutput;
        private Button btnEncrypt, btnDecrypt, btnRecognize, btnBackFromEncrypt;
        private Label lblShift, lblCipher;
        private const int MAX_TEXT_LENGTH = 10000;

        // Элементы подписи
        private TextBox txtSignInput, txtSignature, txtVerifyInput, txtVerifySig;
        private Button btnSign, btnVerify, btnBackFromSignature;
        private Label lblVerifyResult;

        // Элементы истории
        private ListBox lstHistory;
        private Button btnBackFromHistory;

        // Элементы логов
        private TextBox txtLog;
        private Button btnRefreshLog, btnBackFromLog;

        // Трей и хоткей
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;
        private bool isTrayMode = false;

        public MainForm()
        {
            InitializeComponent();
            this.BackColor = Color.FromArgb(45, 74, 45); // тёмно-зелёный фон
            this.Font = new Font("Consolas", 10);
            this.FormClosing += MainForm_FormClosing;
            this.Resize += MainForm_Resize;

            // Загружаем фоновое изображение, если есть
            LoadBackgroundImage();

            CreateMenuPanel();
            CreateEncryptPanel();
            CreateSignaturePanel();
            CreateHistoryPanel();
            CreateLogPanel();

            // Показываем меню
            ShowMenu();

            // Инициализация трея
            SetupTray();

            // Регистрация горячей клавиши
            bool hotkeyRegistered = HotkeyManager.Register(this.Handle, Keys.E, true, true, false); // Ctrl+Shift+E
            if (!hotkeyRegistered)
            {
                MessageBox.Show("Не удалось зарегистрировать горячую клавишу. Возможно, она уже используется другим приложением.",
                                "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            // Загрузка данных
            LoadHistory();
            LoadLog();

            // Инициализация подсказок
            InitializeToolTips();
        }

        // ---------- Загрузка фонового изображения ----------
        private void LoadBackgroundImage()
        {
            try
            {
                string bgPath = Path.Combine(Application.StartupPath, "Resources", "camo_bg.jpg");
                if (File.Exists(bgPath))
                {
                    this.BackgroundImage = Image.FromFile(bgPath);
                    this.BackgroundImageLayout = ImageLayout.Stretch;
                }
            }
            catch { /* если нет фона - используем цвет */ }
        }

        // ---------- Инициализация подсказок (BUG-008) ----------
        private void InitializeToolTips()
        {
            toolTip = new ToolTip();
            toolTip.SetToolTip(btnEncryptMenu, "Перейти к шифрованию текста");
            toolTip.SetToolTip(btnSignatureMenu, "Создать или проверить цифровую подпись");
            toolTip.SetToolTip(btnHistoryMenu, "Просмотреть историю операций шифрования");
            toolTip.SetToolTip(btnLogMenu, "Просмотреть журнал всех действий");
        }

        // ---------- Сохранение соотношения сторон (BUG-003) ----------
        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (this.WindowState != FormWindowState.Normal) return;

            // Сохраняем соотношение 16:9
            int newWidth = (int)(this.Height * 16.0 / 9.0);

            // Проверяем, что новый размер не меньше минимального
            if (newWidth < this.MinimumSize.Width)
            {
                newWidth = this.MinimumSize.Width;
                this.Height = (int)(newWidth * 9.0 / 16.0);
            }

            if (this.Width != newWidth)
            {
                this.Width = newWidth;
            }
        }

        // ---------- Навигация ----------
        private void ShowMenu()
        {
            menuPanel.Visible = true;
            encryptPanel.Visible = false;
            signaturePanel.Visible = false;
            historyPanel.Visible = false;
            logPanel.Visible = false;
            this.Text = "Змеиный кодек — Главное меню";
        }

        private void ShowEncrypt()
        {
            menuPanel.Visible = false;
            encryptPanel.Visible = true;
            signaturePanel.Visible = false;
            historyPanel.Visible = false;
            logPanel.Visible = false;
            this.Text = "Змеиный кодек — Шифрование";
        }

        private void ShowSignature()
        {
            menuPanel.Visible = false;
            encryptPanel.Visible = false;
            signaturePanel.Visible = true;
            historyPanel.Visible = false;
            logPanel.Visible = false;
            this.Text = "Змеиный кодек — Подпись";
        }

        private void ShowHistory()
        {
            menuPanel.Visible = false;
            encryptPanel.Visible = false;
            signaturePanel.Visible = false;
            historyPanel.Visible = true;
            logPanel.Visible = false;
            LoadHistory(); // обновляем перед показом
            this.Text = "Змеиный кодек — История операций";
        }

        private void ShowLog()
        {
            menuPanel.Visible = false;
            encryptPanel.Visible = false;
            signaturePanel.Visible = false;
            historyPanel.Visible = false;
            logPanel.Visible = true;
            LoadLog(); // обновляем перед показом
            this.Text = "Змеиный кодек — Журнал";
        }

        // ---------- Создание панелей ----------
        private void CreateMenuPanel()
        {
            menuPanel = new Panel
            {
                Size = new Size(400, 300),
                Location = new Point((this.ClientSize.Width - 400) / 2, (this.ClientSize.Height - 300) / 2),
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.None
            };

            btnEncryptMenu = CreateMenuButton("ШИФРОВАНИЕ", new Point(0, 0));
            btnEncryptMenu.Click += (s, e) => ShowEncrypt();

            btnSignatureMenu = CreateMenuButton("ПОДПИСЬ", new Point(0, 60));
            btnSignatureMenu.Click += (s, e) => ShowSignature();

            btnHistoryMenu = CreateMenuButton("ИСТОРИЯ", new Point(0, 120));
            btnHistoryMenu.Click += (s, e) => ShowHistory();

            btnLogMenu = CreateMenuButton("ЖУРНАЛ", new Point(0, 180));
            btnLogMenu.Click += (s, e) => ShowLog();

            menuPanel.Controls.AddRange(new Control[] { btnEncryptMenu, btnSignatureMenu, btnHistoryMenu, btnLogMenu });
            this.Controls.Add(menuPanel);
        }

        private Button CreateMenuButton(string text, Point location)
        {
            var btn = new Button
            {
                Text = text,
                Location = location,
                Size = new Size(200, 50),
                Font = new Font("Consolas", 14, FontStyle.Bold),
                BackColor = Color.FromArgb(79, 122, 79),
                ForeColor = Color.FromArgb(224, 240, 208),
                FlatStyle = FlatStyle.Flat
            };

            // Попытка загрузить иконку для кнопки (если есть)
            Image icon = LoadImage("menu_icon.png");
            if (icon != null)
            {
                btn.Image = icon;
                btn.ImageAlign = ContentAlignment.MiddleLeft;
                btn.TextImageRelation = TextImageRelation.ImageBeforeText;
                btn.Padding = new Padding(5, 0, 0, 0);
            }

            return btn;
        }

        private void CreateEncryptPanel()
        {
            encryptPanel = new Panel
            {
                Size = this.ClientSize,
                Location = new Point(0, 0),
                BackColor = Color.Transparent,
                Visible = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            encryptPanel.Resize += EncryptPanel_Resize;

            // Кнопка Назад с возможной иконкой
            btnBackFromEncrypt = CreateStyledButton("← НАЗАД", 10, 10, 100, 30, "back_arrow.png");
            btnBackFromEncrypt.Click += (s, e) => ShowMenu();
            encryptPanel.Controls.Add(btnBackFromEncrypt);

            // Выбор шифра
            lblCipher = new Label
            {
                Text = "Шифр:",
                Location = new Point(20, 60),
                Size = new Size(80, 25),
                ForeColor = Color.FromArgb(184, 217, 166),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };

            cmbCipher = new ComboBox
            {
                Location = new Point(110, 60),
                Size = new Size(150, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(30, 43, 30),
                ForeColor = Color.FromArgb(184, 217, 166),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            cmbCipher.Items.AddRange(new[] { "Цезарь", "ROT-n", "Азбука Морзе", "Двоичный код", "A1Z26", "Base32", "Base64", "ASCII" });
            cmbCipher.SelectedIndex = 1;
            cmbCipher.SelectedIndexChanged += CmbCipher_SelectedIndexChanged;

            // Сдвиг (изначально виден, но скроется, если не нужен)
            lblShift = new Label
            {
                Text = "Сдвиг:",
                Location = new Point(280, 60),
                Size = new Size(60, 25),
                ForeColor = Color.FromArgb(184, 217, 166),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };

            nudShift = new NumericUpDown
            {
                Location = new Point(350, 60),
                Size = new Size(60, 25),
                Minimum = 1,
                Maximum = 25,
                Value = 13,
                BackColor = Color.FromArgb(30, 43, 30),
                ForeColor = Color.FromArgb(184, 217, 166),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };

            // Поле ввода
            var lblInput = new Label
            {
                Text = "Ввод:",
                Location = new Point(20, 100),
                Size = new Size(80, 25),
                ForeColor = Color.FromArgb(184, 217, 166),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };

            txtInput = new TextBox
            {
                Location = new Point(110, 100),
                Size = new Size(this.ClientSize.Width - 130, 100),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.FromArgb(30, 43, 30),
                ForeColor = Color.FromArgb(184, 217, 166),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            // Поле вывода с исправленным масштабированием (BUG-004)
            var lblOutput = new Label
            {
                Text = "Вывод:",
                Location = new Point(20, 210),
                Size = new Size(80, 25),
                ForeColor = Color.FromArgb(184, 217, 166),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };

            txtOutput = new TextBox
            {
                Location = new Point(110, 210),
                Size = new Size(this.ClientSize.Width - 130, this.ClientSize.Height - 320),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                BackColor = Color.FromArgb(30, 43, 30),
                ForeColor = Color.FromArgb(184, 217, 166),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            // Кнопки действий
            btnEncrypt = CreateStyledButton("ЗАШИФРОВАТЬ", 110, this.ClientSize.Height - 90, 120, 30, "encrypt.png");
            btnEncrypt.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnEncrypt.Click += BtnEncrypt_Click;

            btnDecrypt = CreateStyledButton("ДЕШИФРОВАТЬ", 240, this.ClientSize.Height - 90, 120, 30, "decrypt.png");
            btnDecrypt.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnDecrypt.Click += BtnDecrypt_Click;

            btnRecognize = CreateStyledButton("РАСПОЗНАТЬ", 370, this.ClientSize.Height - 90, 120, 30, "recognize.png");
            btnRecognize.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnRecognize.Click += BtnRecognize_Click;

            encryptPanel.Controls.AddRange(new Control[] {
                lblCipher, cmbCipher, lblShift, nudShift,
                lblInput, txtInput, lblOutput, txtOutput,
                btnEncrypt, btnDecrypt, btnRecognize, btnBackFromEncrypt
            });

            this.Controls.Add(encryptPanel);

            // Устанавливаем начальную видимость сдвига
            UpdateShiftVisibility();
        }

        private void EncryptPanel_Resize(object sender, EventArgs e)
        {
            if (txtOutput != null && btnEncrypt != null)
            {
                // Пересчитываем положение кнопок при изменении размера
                btnEncrypt.Top = encryptPanel.Height - 60;
                btnDecrypt.Top = encryptPanel.Height - 60;
                btnRecognize.Top = encryptPanel.Height - 60;

                // Пересчитываем высоту поля вывода
                txtOutput.Height = encryptPanel.Height - 320;
                txtOutput.Width = encryptPanel.Width - 130;
                txtInput.Width = encryptPanel.Width - 130;
            }
        }

        private void CreateSignaturePanel()
        {
            signaturePanel = new Panel
            {
                Size = this.ClientSize,
                Location = new Point(0, 0),
                BackColor = Color.Transparent,
                Visible = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            btnBackFromSignature = CreateStyledButton("← НАЗАД", 10, 10, 100, 30, "back_arrow.png");
            btnBackFromSignature.Click += (s, e) => ShowMenu();
            signaturePanel.Controls.Add(btnBackFromSignature);

            // Подписание
            var lblSignInput = new Label
            {
                Text = "Текст для подписи:",
                Location = new Point(20, 60),
                Size = new Size(150, 25),
                ForeColor = Color.FromArgb(184, 217, 166),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };

            txtSignInput = new TextBox
            {
                Location = new Point(180, 60),
                Size = new Size(this.ClientSize.Width - 200, 60),
                Multiline = true,
                BackColor = Color.FromArgb(30, 43, 30),
                ForeColor = Color.FromArgb(184, 217, 166),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            btnSign = CreateStyledButton("ПОДПИСАТЬ", 180, 130, 120, 30, "sign.png");
            btnSign.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            btnSign.Click += BtnSign_Click;

            var lblSignature = new Label
            {
                Text = "Подпись:",
                Location = new Point(20, 170),
                Size = new Size(80, 25),
                ForeColor = Color.FromArgb(184, 217, 166),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };

            txtSignature = new TextBox
            {
                Location = new Point(110, 170),
                Size = new Size(this.ClientSize.Width - 130, 25),
                ReadOnly = true,
                BackColor = Color.FromArgb(30, 43, 30),
                ForeColor = Color.FromArgb(184, 217, 166),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            // Проверка
            var lblVerifyInput = new Label
            {
                Text = "Текст для проверки:",
                Location = new Point(20, 210),
                Size = new Size(150, 25),
                ForeColor = Color.FromArgb(184, 217, 166),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };

            txtVerifyInput = new TextBox
            {
                Location = new Point(180, 210),
                Size = new Size(this.ClientSize.Width - 200, 60),
                Multiline = true,
                BackColor = Color.FromArgb(30, 43, 30),
                ForeColor = Color.FromArgb(184, 217, 166),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            var lblVerifySig = new Label
            {
                Text = "Подпись:",
                Location = new Point(20, 280),
                Size = new Size(80, 25),
                ForeColor = Color.FromArgb(184, 217, 166),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };

            txtVerifySig = new TextBox
            {
                Location = new Point(110, 280),
                Size = new Size(300, 25),
                BackColor = Color.FromArgb(30, 43, 30),
                ForeColor = Color.FromArgb(184, 217, 166),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };

            btnVerify = CreateStyledButton("ПРОВЕРИТЬ", 420, 280, 100, 25, "verify.png");
            btnVerify.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            btnVerify.Click += BtnVerify_Click;

            lblVerifyResult = new Label
            {
                Location = new Point(110, 310),
                Size = new Size(300, 25),
                ForeColor = Color.FromArgb(184, 217, 166),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };

            signaturePanel.Controls.AddRange(new Control[] {
                lblSignInput, txtSignInput, btnSign, lblSignature, txtSignature,
                lblVerifyInput, txtVerifyInput, lblVerifySig, txtVerifySig,
                btnVerify, lblVerifyResult, btnBackFromSignature
            });

            this.Controls.Add(signaturePanel);
        }

        private void CreateHistoryPanel()
        {
            historyPanel = new Panel
            {
                Size = this.ClientSize,
                Location = new Point(0, 0),
                BackColor = Color.Transparent,
                Visible = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            btnBackFromHistory = CreateStyledButton("← НАЗАД", 10, 10, 100, 30, "back_arrow.png");
            btnBackFromHistory.Click += (s, e) => ShowMenu();
            historyPanel.Controls.Add(btnBackFromHistory);

            lstHistory = new ListBox
            {
                Location = new Point(20, 50),
                Size = new Size(this.ClientSize.Width - 40, this.ClientSize.Height - 80),
                BackColor = Color.FromArgb(30, 43, 30),
                ForeColor = Color.FromArgb(184, 217, 166),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            historyPanel.Controls.Add(lstHistory);
            this.Controls.Add(historyPanel);
        }

        private void CreateLogPanel()
        {
            logPanel = new Panel
            {
                Size = this.ClientSize,
                Location = new Point(0, 0),
                BackColor = Color.Transparent,
                Visible = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            btnBackFromLog = CreateStyledButton("← НАЗАД", 10, 10, 100, 30, "back_arrow.png");
            btnBackFromLog.Click += (s, e) => ShowMenu();
            logPanel.Controls.Add(btnBackFromLog);

            txtLog = new TextBox
            {
                Location = new Point(20, 50),
                Size = new Size(this.ClientSize.Width - 40, this.ClientSize.Height - 100),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                BackColor = Color.FromArgb(30, 43, 30),
                ForeColor = Color.FromArgb(184, 217, 166),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            logPanel.Controls.Add(txtLog);

            btnRefreshLog = CreateStyledButton("ОБНОВИТЬ", 20, this.ClientSize.Height - 40, 120, 30, "refresh.png");
            btnRefreshLog.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnRefreshLog.Click += (s, e) => LoadLog();
            logPanel.Controls.Add(btnRefreshLog);

            this.Controls.Add(logPanel);
        }

        // Вспомогательный метод для создания кнопок с иконками
        private Button CreateStyledButton(string text, int x, int y, int width, int height, string iconFile = null)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, height),
                BackColor = Color.FromArgb(79, 122, 79),
                ForeColor = Color.FromArgb(224, 240, 208),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Consolas", 9, FontStyle.Bold)
            };

            if (!string.IsNullOrEmpty(iconFile))
            {
                Image icon = LoadImage(iconFile);
                if (icon != null)
                {
                    btn.Image = icon;
                    btn.ImageAlign = ContentAlignment.MiddleLeft;
                    btn.TextImageRelation = TextImageRelation.ImageBeforeText;
                    btn.Padding = new Padding(5, 0, 0, 0);
                }
            }

            return btn;
        }

        // Загрузка изображения из папки Resources
        private Image LoadImage(string filename)
        {
            try
            {
                string path = Path.Combine(Application.StartupPath, "Resources", filename);
                if (File.Exists(path))
                    return Image.FromFile(path);
            }
            catch { }
            return null;
        }

        // ---------- Логика отображения сдвига ----------
        private void CmbCipher_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateShiftVisibility();
        }

        private void UpdateShiftVisibility()
        {
            string selected = cmbCipher.SelectedItem.ToString();
            bool showShift = selected == "Цезарь" || selected == "ROT-n";

            if (lblShift != null)
                lblShift.Visible = showShift;
            if (nudShift != null)
                nudShift.Visible = showShift;
        }

        // ---------- Проверка на пустые поля (BUG-011) ----------
        private bool ValidateInput(string input, string operationName)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                MessageBox.Show($"Введите текст для {operationName}",
                                "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Проверка на максимальную длину (BUG-006)
            if (input.Length > MAX_TEXT_LENGTH)
            {
                MessageBox.Show($"Текст слишком длинный. Максимальная длина: {MAX_TEXT_LENGTH} символов.",
                                "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        // ---------- Логика шифрования ----------
        private void BtnEncrypt_Click(object sender, EventArgs e)
        {
            string text = txtInput.Text;

            // Проверка на пустые поля (BUG-011)
            if (!ValidateInput(text, "шифрования")) return;

            string cipher = cmbCipher.SelectedItem.ToString();
            int shift = (int)nudShift.Value;
            string result = "";

            try
            {
                switch (cipher)
                {
                    case "Цезарь": result = Ciphers.Caesar(text, shift, true); break;
                    case "ROT-n": result = Ciphers.Rot(text, shift, true); break;
                    case "Азбука Морзе": result = Ciphers.MorseEncode(text); break;
                    case "Двоичный код": result = Ciphers.BinaryEncode(text); break;
                    case "A1Z26": result = Ciphers.A1Z26Encode(text); break;
                    case "Base32": result = Ciphers.Base32Encode(text); break;
                    case "Base64": result = Ciphers.Base64Encode(text); break;
                    case "ASCII": result = Ciphers.AsciiEncode(text); break;
                }
                txtOutput.Text = result;
                Logger.Log("ENCRYPT", $"{cipher}: {Truncate(text)}");
                RadioHistory.AddEntry("ENCRYPT", cipher, text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDecrypt_Click(object sender, EventArgs e)
        {
            string text = txtInput.Text;

            // Проверка на пустые поля (BUG-011)
            if (!ValidateInput(text, "дешифрования")) return;

            string cipher = cmbCipher.SelectedItem.ToString();
            int shift = (int)nudShift.Value;
            string result = "";

            try
            {
                switch (cipher)
                {
                    case "Цезарь": result = Ciphers.Caesar(text, shift, false); break;
                    case "ROT-n": result = Ciphers.Rot(text, shift, false); break;
                    case "Азбука Морзе": result = Ciphers.MorseDecode(text); break;
                    case "Двоичный код": result = Ciphers.BinaryDecode(text); break;
                    case "A1Z26": result = Ciphers.A1Z26Decode(text); break;
                    case "Base32": result = Ciphers.Base32Decode(text); break;
                    case "Base64": result = Ciphers.Base64Decode(text); break;
                    case "ASCII": result = Ciphers.AsciiDecode(text); break;
                }
                txtOutput.Text = result;
                Logger.Log("DECRYPT", $"{cipher}: {Truncate(text)}");
                RadioHistory.AddEntry("DECRYPT", cipher, text);
            }
            catch (FormatException)
            {
                MessageBox.Show("Ошибка формата: введённый текст не соответствует выбранному шифру.",
                                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnRecognize_Click(object sender, EventArgs e)
        {
            string text = txtInput.Text;

            // Проверка на пустые поля (BUG-011)
            if (!ValidateInput(text, "распознавания")) return;

            string guess = "Не удалось определить";

            if (text.All(c => "01 ".Contains(c)) && text.Contains("0") && text.Contains("1"))
                guess = "Двоичный код";
            else if (text.Replace(" ", "").All(c => ".-/".Contains(c)) && text.Contains(".") && text.Contains("-"))
                guess = "Азбука Морзе";
            else if (text.Split('-').All(p => int.TryParse(p, out int n) && n >= 1 && n <= 26) && text.Contains("-"))
                guess = "A1Z26";
            else if (text.Length % 4 == 0 && text.All(c => char.IsLetterOrDigit(c) || c == '='))
                guess = "Base64 или Base32";
            else if (text.All(c => char.IsDigit(c) || c == ' '))
                guess = "ASCII коды";
            else
                guess = "Возможно Цезарь или ROT";

            MessageBox.Show($"Предположительный шифр: {guess}", "Результат распознавания");
        }

        // ---------- Подпись ----------
        private void BtnSign_Click(object sender, EventArgs e)
        {
            string text = txtSignInput.Text;

            // Проверка на пустые поля (BUG-011)
            if (!ValidateInput(text, "подписи")) return;

            string sig = Signature.SignText(text);
            txtSignature.Text = sig;
            Logger.Log("SIGN", Truncate(text));
            RadioHistory.AddEntry("SIGN", "Signature", text);
        }

        private void BtnVerify_Click(object sender, EventArgs e)
        {
            string text = txtVerifyInput.Text;
            string sig = txtVerifySig.Text;

            // Проверка на пустые поля (BUG-011)
            if (string.IsNullOrWhiteSpace(text))
            {
                MessageBox.Show("Введите текст для проверки", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(sig))
            {
                MessageBox.Show("Введите подпись для проверки", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool ok = Signature.VerifySignature(text, sig);
            lblVerifyResult.Text = ok ? "✓ Подпись верна" : "✗ Подпись недействительна";
            lblVerifyResult.ForeColor = ok ? Color.FromArgb(184, 217, 166) : Color.Red;
        }

        // ---------- История и логи (BUG-005) ----------
        private void LoadHistory()
        {
            if (lstHistory == null) return;
            lstHistory.Items.Clear();

            // Фильтруем только операции шифрования и дешифрования
            var historyEntries = RadioHistory.GetAll()
                .Where(e => e.OperationType == "ENCRYPT" || e.OperationType == "DECRYPT");

            foreach (var entry in historyEntries)
            {
                lstHistory.Items.Add($"{entry.Frequency:F2} MHz | {entry.OperationType} | {entry.CipherName} | {entry.Preview} [{entry.Timestamp:HH:mm:ss}]");
            }
        }

        private void LoadLog()
        {
            if (txtLog == null) return;
            txtLog.Text = Logger.GetLogs(100);
        }

        private string Truncate(string s, int maxLen = 30)
        {
            if (string.IsNullOrEmpty(s)) return s;
            if (s.Length <= maxLen) return s;
            return s.Substring(0, maxLen) + "...";
        }

        // ---------- Трей (BUG-009, BUG-010) ----------
        private void SetupTray()
        {
            trayIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application,
                Text = "Змеиный кодек",
                Visible = true
            };

            // Создаём контекстное меню для трея
            trayMenu = new ContextMenuStrip();

            var showMenuItem = new ToolStripMenuItem("Показать окно");
            showMenuItem.Click += (s, e) => ShowFromTray();

            var exitMenuItem = new ToolStripMenuItem("Выход");
            exitMenuItem.Click += (s, e) =>
            {
                trayIcon.Visible = false;
                Application.Exit();
            };

            trayMenu.Items.Add(showMenuItem);
            trayMenu.Items.Add(new ToolStripSeparator());
            trayMenu.Items.Add(exitMenuItem);

            trayIcon.ContextMenuStrip = trayMenu;
            trayIcon.DoubleClick += (s, e) => ShowFromTray();
        }

        private void ShowFromTray()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
            isTrayMode = false;
        }

        // ---------- Обработка закрытия формы (BUG-010) ----------
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && !isTrayMode)
            {
                // Сворачиваем в трей вместо закрытия
                e.Cancel = true;
                this.Hide();
                trayIcon.ShowBalloonTip(1000, "Змеиный кодек",
                                         "Приложение свёрнуто в системный трей",
                                         ToolTipIcon.Info);
            }
        }

        // ---------- Горячая клавиша (BUG-007) ----------
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == HotkeyManager.WM_HOTKEY && m.WParam.ToInt32() == HotkeyManager.HOTKEY_ID)
            {
                if (Clipboard.ContainsText())
                {
                    try
                    {
                        string text = Clipboard.GetText();
                        string encrypted = Ciphers.Rot(text, 13, true); // ROT13 для шифрования
                        Clipboard.SetText(encrypted);
                        Logger.Log("QUICK_ENCRYPT", "Буфер обмена ROT13");

                        // Показываем уведомление, если приложение свёрнуто
                        if (this.WindowState == FormWindowState.Minimized || !this.Visible)
                        {
                            trayIcon.ShowBalloonTip(1000, "Быстрое шифрование",
                                                     "Текст в буфере обмена зашифрован ROT13",
                                                     ToolTipIcon.Info);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("QUICK_ENCRYPT_ERROR", ex.Message);
                    }
                }
            }
        }
    }
}