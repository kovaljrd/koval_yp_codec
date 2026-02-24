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
        private Panel _menuPanel;
        private Panel _encryptPanel;
        private Panel _signaturePanel;
        private Panel _historyPanel;
        private Panel _logPanel;

        // Элементы главного меню
        private Button _btnEncryptMenu, _btnSignatureMenu, _btnHistoryMenu, _btnLogMenu;
        private ToolTip _toolTip;

        // Элементы шифрования
        private ComboBox _cmbCipher;
        private NumericUpDown _nudShift;
        private TextBox _txtInput, _txtOutput;
        private Button _btnEncrypt, _btnDecrypt, _btnRecognize, _btnBackFromEncrypt;
        private Label _lblShift, _lblCipher;
        private const int MAX_TEXT_LENGTH = 10000;

        // НОВОЕ: Элементы выбора раскладки
        private GroupBox _gbLayout;
        private RadioButton _rbLayoutAuto;
        private RadioButton _rbLayoutLatin;
        private RadioButton _rbLayoutCyrillic;
        private KeyboardLayoutMode _currentLayout = KeyboardLayoutMode.Auto;

        // Элементы подписи
        private TextBox _txtSignInput, _txtSignature, _txtVerifyInput, _txtVerifySig;
        private Button _btnSign, _btnVerify, _btnBackFromSignature;
        private Label _lblVerifyResult;

        // Элементы истории
        private ListBox _lstHistory;
        private Button _btnBackFromHistory;
        private Button _btnClearHistory;      // НОВОЕ: кнопка очистки истории
        private Button _btnExportHistory;     // НОВОЕ: кнопка экспорта истории

        // Элементы логов
        private TextBox _txtLog;
        private Button _btnRefreshLog, _btnBackFromLog;
        private Button _btnClearLog;           // НОВОЕ: кнопка очистки журнала
        private Button _btnExportLog;          // НОВОЕ: кнопка экспорта журнала
        private Label _lblLogStats;            // НОВОЕ: статистика журнала

        // Трей и хоткей
        private NotifyIcon _trayIcon;
        private ContextMenuStrip _trayMenu;
        private bool _isTrayMode = false;

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

        // ---------- Инициализация подсказок ----------
        private void InitializeToolTips()
        {
            _toolTip = new ToolTip();
            _toolTip.SetToolTip(_btnEncryptMenu, "Перейти к шифрованию текста");
            _toolTip.SetToolTip(_btnSignatureMenu, "Создать или проверить цифровую подпись");
            _toolTip.SetToolTip(_btnHistoryMenu, "Просмотреть историю операций шифрования");
            _toolTip.SetToolTip(_btnLogMenu, "Просмотреть журнал всех действий");

            // НОВОЕ: подсказки для новых кнопок
            if (_btnClearHistory != null)
                _toolTip.SetToolTip(_btnClearHistory, "Очистить всю историю операций");
            if (_btnExportHistory != null)
                _toolTip.SetToolTip(_btnExportHistory, "Экспортировать историю в файл");
            if (_btnClearLog != null)
                _toolTip.SetToolTip(_btnClearLog, "Очистить журнал операций");
            if (_btnExportLog != null)
                _toolTip.SetToolTip(_btnExportLog, "Экспортировать журнал в файл");
        }

        // ---------- Сохранение соотношения сторон ----------
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
            _menuPanel.Visible = true;
            _encryptPanel.Visible = false;
            _signaturePanel.Visible = false;
            _historyPanel.Visible = false;
            _logPanel.Visible = false;
            this.Text = "Змеиный кодек — Главное меню";
        }

        private void ShowEncrypt()
        {
            _menuPanel.Visible = false;
            _encryptPanel.Visible = true;
            _signaturePanel.Visible = false;
            _historyPanel.Visible = false;
            _logPanel.Visible = false;
            this.Text = "Змеиный кодек — Шифрование";
        }

        private void ShowSignature()
        {
            _menuPanel.Visible = false;
            _encryptPanel.Visible = false;
            _signaturePanel.Visible = true;
            _historyPanel.Visible = false;
            _logPanel.Visible = false;
            this.Text = "Змеиный кодек — Подпись";
        }

        private void ShowHistory()
        {
            _menuPanel.Visible = false;
            _encryptPanel.Visible = false;
            _signaturePanel.Visible = false;
            _historyPanel.Visible = true;
            _logPanel.Visible = false;
            LoadHistory(); // обновляем перед показом
            this.Text = "Змеиный кодек — История операций";
        }

        private void ShowLog()
        {
            _menuPanel.Visible = false;
            _encryptPanel.Visible = false;
            _signaturePanel.Visible = false;
            _historyPanel.Visible = false;
            _logPanel.Visible = true;
            LoadLog(); // обновляем перед показом
            UpdateLogStats(); // НОВОЕ: обновляем статистику
            this.Text = "Змеиный кодек — Журнал";
        }

        // ---------- Создание панелей ----------
        private void CreateMenuPanel()
        {
            _menuPanel = new Panel
            {
                Size = new Size(400, 300),
                Location = new Point((this.ClientSize.Width - 400) / 2, (this.ClientSize.Height - 300) / 2),
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.None
            };

            _btnEncryptMenu = CreateMenuButton("ШИФРОВАНИЕ", new Point(0, 0));
            _btnEncryptMenu.Click += (s, e) => ShowEncrypt();

            _btnSignatureMenu = CreateMenuButton("ПОДПИСЬ", new Point(0, 60));
            _btnSignatureMenu.Click += (s, e) => ShowSignature();

            _btnHistoryMenu = CreateMenuButton("ИСТОРИЯ", new Point(0, 120));
            _btnHistoryMenu.Click += (s, e) => ShowHistory();

            _btnLogMenu = CreateMenuButton("ЖУРНАЛ", new Point(0, 180));
            _btnLogMenu.Click += (s, e) => ShowLog();

            _menuPanel.Controls.AddRange(new Control[] { _btnEncryptMenu, _btnSignatureMenu, _btnHistoryMenu, _btnLogMenu });
            this.Controls.Add(_menuPanel);
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
            _encryptPanel = new Panel
            {
                Size = this.ClientSize,
                Location = new Point(0, 0),
                BackColor = Color.Transparent,
                Visible = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            _encryptPanel.Resize += EncryptPanel_Resize;

            // Кнопка Назад с возможной иконкой
            _btnBackFromEncrypt = CreateStyledButton("← НАЗАД", 10, 10, 100, 30, "back_arrow.png");
            _btnBackFromEncrypt.Click += (s, e) => ShowMenu();
            _encryptPanel.Controls.Add(_btnBackFromEncrypt);

            // Выбор шифра
            _lblCipher = new Label
            {
                Text = "Шифр:",
                Location = new Point(20, 60),
                Size = new Size(80, 25),
                ForeColor = Color.FromArgb(184, 217, 166),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };

            _cmbCipher = new ComboBox
            {
                Location = new Point(110, 60),
                Size = new Size(150, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(30, 43, 30),
                ForeColor = Color.FromArgb(184, 217, 166),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            _cmbCipher.Items.AddRange(new[] { "Цезарь", "ROT-n", "Азбука Морзе", "Двоичный код", "A1Z26", "Base32", "Base64", "ASCII" });
            _cmbCipher.SelectedIndex = 1;
            _cmbCipher.SelectedIndexChanged += CmbCipher_SelectedIndexChanged;

            // Сдвиг (изначально виден, но скроется, если не нужен)
            _lblShift = new Label
            {
                Text = "Сдвиг:",
                Location = new Point(280, 60),
                Size = new Size(60, 25),
                ForeColor = Color.FromArgb(184, 217, 166),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };

            _nudShift = new NumericUpDown
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

            // НОВОЕ: Группа выбора раскладки
            _gbLayout = new GroupBox
            {
                Text = "Раскладка",
                Location = new Point(420, 45),
                Size = new Size(240, 50),
                ForeColor = Color.FromArgb(184, 217, 166),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };

            _rbLayoutAuto = new RadioButton
            {
                Text = "Авто",
                Location = new Point(10, 20),
                Size = new Size(60, 25),
                ForeColor = Color.FromArgb(184, 217, 166),
                Checked = true
            };
            _rbLayoutAuto.CheckedChanged += LayoutRadioButton_CheckedChanged;

            _rbLayoutLatin = new RadioButton
            {
                Text = "Латиница",
                Location = new Point(80, 20),
                Size = new Size(80, 25),
                ForeColor = Color.FromArgb(184, 217, 166)
            };
            _rbLayoutLatin.CheckedChanged += LayoutRadioButton_CheckedChanged;

            _rbLayoutCyrillic = new RadioButton
            {
                Text = "Кириллица",
                Location = new Point(170, 20),
                Size = new Size(80, 25),
                ForeColor = Color.FromArgb(184, 217, 166)
            };
            _rbLayoutCyrillic.CheckedChanged += LayoutRadioButton_CheckedChanged;

            _gbLayout.Controls.AddRange(new Control[] { _rbLayoutAuto, _rbLayoutLatin, _rbLayoutCyrillic });

            // Поле ввода
            var lblInput = new Label
            {
                Text = "Ввод:",
                Location = new Point(20, 100),
                Size = new Size(80, 25),
                ForeColor = Color.FromArgb(184, 217, 166),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };

            _txtInput = new TextBox
            {
                Location = new Point(110, 100),
                Size = new Size(this.ClientSize.Width - 130, 100),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.FromArgb(30, 43, 30),
                ForeColor = Color.FromArgb(184, 217, 166),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            // Поле вывода
            var lblOutput = new Label
            {
                Text = "Вывод:",
                Location = new Point(20, 210),
                Size = new Size(80, 25),
                ForeColor = Color.FromArgb(184, 217, 166),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };

            _txtOutput = new TextBox
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
            _btnEncrypt = CreateStyledButton("ЗАШИФРОВАТЬ", 110, this.ClientSize.Height - 90, 120, 30, "encrypt.png");
            _btnEncrypt.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            _btnEncrypt.Click += BtnEncrypt_Click;

            _btnDecrypt = CreateStyledButton("ДЕШИФРОВАТЬ", 240, this.ClientSize.Height - 90, 120, 30, "decrypt.png");
            _btnDecrypt.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            _btnDecrypt.Click += BtnDecrypt_Click;

            _btnRecognize = CreateStyledButton("РАСПОЗНАТЬ", 370, this.ClientSize.Height - 90, 120, 30, "recognize.png");
            _btnRecognize.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            _btnRecognize.Click += BtnRecognize_Click;

            _encryptPanel.Controls.AddRange(new Control[] {
                _lblCipher, _cmbCipher, _lblShift, _nudShift, _gbLayout,
                lblInput, _txtInput, lblOutput, _txtOutput,
                _btnEncrypt, _btnDecrypt, _btnRecognize, _btnBackFromEncrypt
            });

            this.Controls.Add(_encryptPanel);

            // Устанавливаем начальную видимость сдвига
            UpdateShiftVisibility();
        }

        // НОВОЕ: Обработчик изменения выбора раскладки
        private void LayoutRadioButton_CheckedChanged(object senderObj, EventArgs eventArgs)
        {
            if (_rbLayoutAuto.Checked)
                _currentLayout = KeyboardLayoutMode.Auto;
            else if (_rbLayoutLatin.Checked)
                _currentLayout = KeyboardLayoutMode.Latin;
            else if (_rbLayoutCyrillic.Checked)
                _currentLayout = KeyboardLayoutMode.Cyrillic;
        }

        private void EncryptPanel_Resize(object sender, EventArgs e)
        {
            if (_txtOutput != null && _btnEncrypt != null)
            {
                // Пересчитываем положение кнопок при изменении размера
                _btnEncrypt.Top = _encryptPanel.Height - 60;
                _btnDecrypt.Top = _encryptPanel.Height - 60;
                _btnRecognize.Top = _encryptPanel.Height - 60;

                // Пересчитываем высоту поля вывода
                _txtOutput.Height = _encryptPanel.Height - 320;
                _txtOutput.Width = _encryptPanel.Width - 130;
                _txtInput.Width = _encryptPanel.Width - 130;
            }
        }

        private void CreateSignaturePanel()
        {
            _signaturePanel = new Panel
            {
                Size = this.ClientSize,
                Location = new Point(0, 0),
                BackColor = Color.Transparent,
                Visible = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            _btnBackFromSignature = CreateStyledButton("← НАЗАД", 10, 10, 100, 30, "back_arrow.png");
            _btnBackFromSignature.Click += (s, e) => ShowMenu();
            _signaturePanel.Controls.Add(_btnBackFromSignature);

            // Подписание
            var lblSignInput = new Label
            {
                Text = "Текст для подписи:",
                Location = new Point(20, 60),
                Size = new Size(150, 25),
                ForeColor = Color.FromArgb(184, 217, 166),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };

            _txtSignInput = new TextBox
            {
                Location = new Point(180, 60),
                Size = new Size(this.ClientSize.Width - 200, 60),
                Multiline = true,
                BackColor = Color.FromArgb(30, 43, 30),
                ForeColor = Color.FromArgb(184, 217, 166),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            _btnSign = CreateStyledButton("ПОДПИСАТЬ", 180, 130, 120, 30, "sign.png");
            _btnSign.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            _btnSign.Click += BtnSign_Click;

            var lblSignature = new Label
            {
                Text = "Подпись:",
                Location = new Point(20, 170),
                Size = new Size(80, 25),
                ForeColor = Color.FromArgb(184, 217, 166),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };

            _txtSignature = new TextBox
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

            _txtVerifyInput = new TextBox
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

            _txtVerifySig = new TextBox
            {
                Location = new Point(110, 280),
                Size = new Size(300, 25),
                BackColor = Color.FromArgb(30, 43, 30),
                ForeColor = Color.FromArgb(184, 217, 166),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };

            _btnVerify = CreateStyledButton("ПРОВЕРИТЬ", 420, 280, 100, 25, "verify.png");
            _btnVerify.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            _btnVerify.Click += BtnVerify_Click;

            _lblVerifyResult = new Label
            {
                Location = new Point(110, 310),
                Size = new Size(300, 25),
                ForeColor = Color.FromArgb(184, 217, 166),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };

            _signaturePanel.Controls.AddRange(new Control[] {
                lblSignInput, _txtSignInput, _btnSign, lblSignature, _txtSignature,
                lblVerifyInput, _txtVerifyInput, lblVerifySig, _txtVerifySig,
                _btnVerify, _lblVerifyResult, _btnBackFromSignature
            });

            this.Controls.Add(_signaturePanel);
        }

        private void CreateHistoryPanel()
        {
            _historyPanel = new Panel
            {
                Size = this.ClientSize,
                Location = new Point(0, 0),
                BackColor = Color.Transparent,
                Visible = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            _btnBackFromHistory = CreateStyledButton("← НАЗАД", 10, 10, 100, 30, "back_arrow.png");
            _btnBackFromHistory.Click += (s, e) => ShowMenu();
            _historyPanel.Controls.Add(_btnBackFromHistory);

            // НОВОЕ: Кнопка очистки истории
            _btnClearHistory = CreateStyledButton("ОЧИСТИТЬ", this.ClientSize.Width - 220, 10, 100, 30, "clear.png");
            _btnClearHistory.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _btnClearHistory.Click += BtnClearHistory_Click;
            _historyPanel.Controls.Add(_btnClearHistory);

            // НОВОЕ: Кнопка экспорта истории
            _btnExportHistory = CreateStyledButton("ЭКСПОРТ", this.ClientSize.Width - 340, 10, 100, 30, "export.png");
            _btnExportHistory.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _btnExportHistory.Click += BtnExportHistory_Click;
            _historyPanel.Controls.Add(_btnExportHistory);

            _lstHistory = new ListBox
            {
                Location = new Point(20, 50),
                Size = new Size(this.ClientSize.Width - 40, this.ClientSize.Height - 80),
                BackColor = Color.FromArgb(30, 43, 30),
                ForeColor = Color.FromArgb(184, 217, 166),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                SelectionMode = SelectionMode.One
            };

            // НОВОЕ: Добавляем контекстное меню для удаления отдельных записей
            var contextMenu = new ContextMenuStrip();
            var deleteMenuItem = new ToolStripMenuItem("Удалить запись");
            deleteMenuItem.Click += DeleteHistoryEntry_Click;
            contextMenu.Items.Add(deleteMenuItem);
            _lstHistory.ContextMenuStrip = contextMenu;

            _historyPanel.Controls.Add(_lstHistory);
            this.Controls.Add(_historyPanel);
        }

        // НОВОЕ: Удаление выбранной записи истории
        private void DeleteHistoryEntry_Click(object senderObj, EventArgs eventArgs)
        {
            if (_lstHistory.SelectedIndex >= 0)
            {
                var result = MessageBox.Show(
                    "Удалить выбранную запись из истории?",
                    "Подтверждение",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Получаем индекс в оригинальном списке (с учётом фильтрации)
                    var historyEntries = RadioHistory.GetAll()
                        .Where(entry => entry.OperationType == "ENCRYPT" || entry.OperationType == "DECRYPT")
                        .ToList();

                    if (_lstHistory.SelectedIndex < historyEntries.Count)
                    {
                        var entryToDelete = historyEntries[_lstHistory.SelectedIndex];
                        // Находим индекс в полном списке
                        int fullIndex = RadioHistory.GetAll().ToList().FindIndex(e =>
                            e.Timestamp == entryToDelete.Timestamp &&
                            e.OperationType == entryToDelete.OperationType &&
                            e.CipherName == entryToDelete.CipherName);

                        if (fullIndex >= 0)
                        {
                            RadioHistory.RemoveEntry(fullIndex);
                            LoadHistory();
                        }
                    }
                }
            }
        }

        private void BtnClearHistory_Click(object senderObj, EventArgs eventArgs)
        {
            var result = MessageBox.Show(
                "Вы уверены, что хотите очистить всю историю операций?\nЭто действие нельзя отменить.",
                "Подтверждение очистки истории",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                RadioHistory.ClearHistory();
                LoadHistory();
            }
        }
        private void BtnExportHistory_Click(object senderObj, EventArgs eventArgs)
        {
            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "JSON файлы (*.json)|*.json|Все файлы (*.*)|*.*";
                saveDialog.DefaultExt = "json";
                saveDialog.FileName = $"history_export_{DateTime.Now:yyyyMMdd_HHmmss}.json";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    if (RadioHistory.ExportToFile(saveDialog.FileName))
                    {
                        MessageBox.Show($"История успешно экспортирована в файл:\n{saveDialog.FileName}",
                                        "Экспорт завершён",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при экспорте истории.",
                                        "Ошибка",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void CreateLogPanel()
        {
            _logPanel = new Panel
            {
                Size = this.ClientSize,
                Location = new Point(0, 0),
                BackColor = Color.Transparent,
                Visible = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            _btnBackFromLog = CreateStyledButton("← НАЗАД", 10, 10, 100, 30, "back_arrow.png");
            _btnBackFromLog.Click += (s, e) => ShowMenu();
            _logPanel.Controls.Add(_btnBackFromLog);

            // НОВОЕ: Кнопка очистки журнала
            _btnClearLog = CreateStyledButton("ОЧИСТИТЬ", this.ClientSize.Width - 220, 10, 100, 30, "clear.png");
            _btnClearLog.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _btnClearLog.Click += BtnClearLog_Click;
            _logPanel.Controls.Add(_btnClearLog);

            // НОВОЕ: Кнопка экспорта журнала
            _btnExportLog = CreateStyledButton("ЭКСПОРТ", this.ClientSize.Width - 340, 10, 100, 30, "export.png");
            _btnExportLog.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _btnExportLog.Click += BtnExportLog_Click;
            _logPanel.Controls.Add(_btnExportLog);

            _txtLog = new TextBox
            {
                Location = new Point(20, 50),
                Size = new Size(this.ClientSize.Width - 40, this.ClientSize.Height - 120),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                BackColor = Color.FromArgb(30, 43, 30),
                ForeColor = Color.FromArgb(184, 217, 166),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            _logPanel.Controls.Add(_txtLog);

            // НОВОЕ: Статистика журнала
            _lblLogStats = new Label
            {
                Location = new Point(20, this.ClientSize.Height - 60),
                Size = new Size(300, 25),
                ForeColor = Color.FromArgb(184, 217, 166),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            _logPanel.Controls.Add(_lblLogStats);

            _btnRefreshLog = CreateStyledButton("ОБНОВИТЬ", 20, this.ClientSize.Height - 40, 120, 30, "refresh.png");
            _btnRefreshLog.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            _btnRefreshLog.Click += (s, e) => LoadLog();
            _logPanel.Controls.Add(_btnRefreshLog);

            this.Controls.Add(_logPanel);
        }

        // НОВОЕ: Очистка журнала
        private void BtnClearLog_Click(object senderObj, EventArgs eventArgs)
        {
            var result = MessageBox.Show(
                "Вы уверены, что хотите очистить весь журнал операций?\nЭто действие нельзя отменить.",
                "Подтверждение очистки журнала",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                Logger.ClearLog();
                LoadLog();
                UpdateLogStats();
            }
        }

        // НОВОЕ: Экспорт журнала в файл
        private void BtnExportLog_Click(object senderObj, EventArgs eventArgs)
        {
            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
                saveDialog.DefaultExt = "txt";
                saveDialog.FileName = $"log_export_{DateTime.Now:yyyyMMdd_HHmmss}.txt";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    if (Logger.ExportLog(saveDialog.FileName))
                    {
                        MessageBox.Show($"Журнал успешно экспортирован в файл:\n{saveDialog.FileName}",
                                        "Экспорт завершён",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при экспорте журнала.",
                                        "Ошибка",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Error);
                    }
                }
            }
        }

        // НОВОЕ: Обновление статистики журнала
        private void UpdateLogStats()
        {
            if (_lblLogStats != null)
            {
                long size = Logger.GetLogSize();
                int lines = Logger.GetLogLineCount();
                string sizeStr = size < 1024 ? $"{size} Б" : $"{size / 1024.0:F1} КБ";
                _lblLogStats.Text = $"Записей: {lines} | Размер: {sizeStr}";
            }
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
            string selected = _cmbCipher.SelectedItem.ToString();
            bool showShift = selected == "Цезарь" || selected == "ROT-n";

            if (_lblShift != null)
                _lblShift.Visible = showShift;
            if (_nudShift != null)
                _nudShift.Visible = showShift;
        }

        // ---------- Проверка на пустые поля ----------
        private bool ValidateInput(string input, string operationName)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                MessageBox.Show($"Введите текст для {operationName}",
                                "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Проверка на максимальную длину
            if (input.Length > MAX_TEXT_LENGTH)
            {
                MessageBox.Show($"Текст слишком длинный. Максимальная длина: {MAX_TEXT_LENGTH} символов.",
                                "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        // ---------- Логика шифрования с поддержкой раскладки ----------
        private void BtnEncrypt_Click(object sender, EventArgs e)
        {
            string text = _txtInput.Text;

            // Проверка на пустые поля
            if (!ValidateInput(text, "шифрования")) return;

            string cipher = _cmbCipher.SelectedItem.ToString();
            int shift = (int)_nudShift.Value;
            string result = "";

            try
            {
                switch (cipher)
                {
                    case "Цезарь":
                        result = Ciphers.Caesar(text, shift, true, _currentLayout);
                        break;
                    case "ROT-n":
                        result = Ciphers.Rot(text, shift, true, _currentLayout);
                        break;
                    case "Азбука Морзе":
                        result = Ciphers.MorseEncode(text, _currentLayout);
                        break;
                    case "Двоичный код":
                        result = Ciphers.BinaryEncode(text);
                        break;
                    case "A1Z26":
                        result = Ciphers.A1Z26Encode(text, _currentLayout);
                        break;
                    case "Base32":
                        result = Ciphers.Base32Encode(text);
                        break;
                    case "Base64":
                        result = Ciphers.Base64Encode(text);
                        break;
                    case "ASCII":
                        result = Ciphers.AsciiEncode(text);
                        break;
                }
                _txtOutput.Text = result;
                Logger.Log("ENCRYPT", $"{cipher}: {Truncate(text)}");
                RadioHistory.AddEntry("ENCRYPT", cipher, text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ---------- Логика дешифрования ----------
        private void BtnDecrypt_Click(object sender, EventArgs e)
        {
            string text = _txtInput.Text;

            // Проверка на пустые поля
            if (!ValidateInput(text, "дешифрования")) return;

            string cipher = _cmbCipher.SelectedItem.ToString();
            int shift = (int)_nudShift.Value;
            string result = "";

            try
            {
                switch (cipher)
                {
                    case "Цезарь":
                        result = Ciphers.Caesar(text, shift, false, _currentLayout);
                        break;
                    case "ROT-n":
                        result = Ciphers.Rot(text, shift, false, _currentLayout);
                        break;
                    case "Азбука Морзе":
                        result = Ciphers.MorseDecode(text);
                        break;
                    case "Двоичный код":
                        result = Ciphers.BinaryDecode(text);
                        break;
                    case "A1Z26":
                        result = Ciphers.A1Z26Decode(text);
                        break;
                    case "Base32":
                        result = Ciphers.Base32Decode(text);
                        break;
                    case "Base64":
                        result = Ciphers.Base64Decode(text);
                        break;
                    case "ASCII":
                        result = Ciphers.AsciiDecode(text);
                        break;
                }
                _txtOutput.Text = result;
                Logger.Log("DECRYPT", $"{cipher}: {Truncate(text)}");
                RadioHistory.AddEntry("DECRYPT", cipher, text);
            }
            catch (FormatException ex)
            {
                MessageBox.Show("Ошибка формата: " + ex.Message,
                                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnRecognize_Click(object sender, EventArgs e)
        {
            string text = _txtInput.Text;

            // Проверка на пустые поля
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
            string text = _txtSignInput.Text;

            // Проверка на пустые поля
            if (!ValidateInput(text, "подписи")) return;

            string sig = Signature.SignText(text);
            _txtSignature.Text = sig;
            Logger.Log("SIGN", Truncate(text));
            RadioHistory.AddEntry("SIGN", "Signature", text);
        }

        private void BtnVerify_Click(object sender, EventArgs e)
        {
            string text = _txtVerifyInput.Text;
            string sig = _txtVerifySig.Text;

            // Проверка на пустые поля
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
            _lblVerifyResult.Text = ok ? "✓ Подпись верна" : "✗ Подпись недействительна";
            _lblVerifyResult.ForeColor = ok ? Color.FromArgb(184, 217, 166) : Color.Red;
        }

        // ---------- История и логи ----------
        private void LoadHistory()
        {
            if (_lstHistory == null) return;
            _lstHistory.Items.Clear();

            // Фильтруем только операции шифрования и дешифрования
            var historyEntries = RadioHistory.GetAll()
                .Where(e => e.OperationType == "ENCRYPT" || e.OperationType == "DECRYPT");

            foreach (var entry in historyEntries)
            {
                _lstHistory.Items.Add($"{entry.Frequency:F2} MHz | {entry.OperationType} | {entry.CipherName} | {entry.Preview} [{entry.Timestamp:HH:mm:ss}]");
            }
        }

        private void LoadLog()
        {
            if (_txtLog == null) return;
            _txtLog.Text = Logger.GetLogs(100);
            UpdateLogStats();
        }

        private string Truncate(string s, int maxLen = 30)
        {
            if (string.IsNullOrEmpty(s)) return s;
            if (s.Length <= maxLen) return s;
            return s.Substring(0, maxLen) + "...";
        }

        // ---------- Трей ----------
        private void SetupTray()
        {
            _trayIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application,
                Text = "Змеиный кодек",
                Visible = true
            };

            // Создаём контекстное меню для трея
            _trayMenu = new ContextMenuStrip();

            var showMenuItem = new ToolStripMenuItem("Показать окно");
            showMenuItem.Click += (s, e) => ShowFromTray();

            var exitMenuItem = new ToolStripMenuItem("Выход");
            exitMenuItem.Click += (s, e) =>
            {
                _trayIcon.Visible = false;
                Application.Exit();
            };

            _trayMenu.Items.Add(showMenuItem);
            _trayMenu.Items.Add(new ToolStripSeparator());
            _trayMenu.Items.Add(exitMenuItem);

            _trayIcon.ContextMenuStrip = _trayMenu;
            _trayIcon.DoubleClick += (s, e) => ShowFromTray();
        }

        private void ShowFromTray()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
            _isTrayMode = false;
        }

        // ---------- Обработка закрытия формы ----------
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && !_isTrayMode)
            {
                // Сворачиваем в трей вместо закрытия
                e.Cancel = true;
                this.Hide();
                _trayIcon.ShowBalloonTip(1000, "Змеиный кодек",
                                         "Приложение свёрнуто в системный трей",
                                         ToolTipIcon.Info);
            }
        }

        // ---------- Горячая клавиша ----------
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
                        string encrypted = Ciphers.Rot(text, 13, true, _currentLayout); // ROT13 для шифрования
                        Clipboard.SetText(encrypted);
                        Logger.Log("QUICK_ENCRYPT", "Буфер обмена ROT13");

                        // Показываем уведомление, если приложение свёрнуто
                        if (this.WindowState == FormWindowState.Minimized || !this.Visible)
                        {
                            _trayIcon.ShowBalloonTip(1000, "Быстрое шифрование",
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