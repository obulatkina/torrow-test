using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace UnitTestTorrowAuthorization
{
    [TestClass]
    public class AuthorizationTest
    {
        const int ZeroButtonIndex = 10;
        const int CleanButtonIndex = 11;
        const int CountCodeNumbersForPhone = 4;
        const int CountCodeNumbersForEmail = 6;

        private static int[] emailCode = new int[] { 1, 2, 3, 4, 5, 6 };
        private static int[] emailInvalidCode = new int[] { 2, 2, 3, 4, 5, 6 };
        private static int[] phoneNumber;
        private static string email;
        private static IWebDriver driver;

        [TestInitialize]
        public void Initialize()
        {
            int[][] phoneNumders = new int[][]
                {
                    new int [] { 9, 9, 9, 6, 9, 7, 5, 5, 8, 9 },
                    new int [] { 9, 6, 7, 8, 8, 6, 7, 8, 6, 7 },
                    new int [] { 9, 8, 6, 3, 4, 5, 5, 8, 5, 7 },
                };

            Random randomizer = new Random();
            int phoneIndex = randomizer.Next(0, phoneNumders.Length);
            phoneNumber = phoneNumders[phoneIndex];

            email = "test@site.net";

            driver = new ChromeDriver("C:/drivers");
            driver.Manage().Window.Maximize();
        }

        public static void WaitShowElement(By selector)
        {
            WebDriverWait iWait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            iWait.Until(webDriver => webDriver.FindElement(selector));
        }

        //[TestMethod]
        public void TestTimeOutWarning()
        {
            driver.Navigate().GoToUrl("https://testqa.torrow.net");

            WaitShowElement(By.Id("auth-phone-input"));
            driver.FindElement(By.Id("auth-phone-input")).Click();

            WriteNumbers(phoneNumber);
            driver.FindElement(By.TagName("ic-arrow-right")).Click();
            WaitShowElement(By.ClassName("tt-card-container"));
            Thread.Sleep(30000);
            var code = GenerateCode(phoneNumber);
            Thread.Sleep(3000);
            WriteNumbers(code);

            // Ошибка появляется не всегда
            var warning = driver.FindElement(By.ClassName("auth-phone-content__warning"));
            if (!warning.Text.Contains("Срок действия кода истек"))
            {
                throw new AssertFailedException("Warning not found");
            }

            Thread.Sleep(3000);

        }

        [TestMethod]
        public void TestPositivePhoneAuthorize()
        {
            driver.Navigate().GoToUrl("https://testqa.torrow.net");

            WaitShowElement(By.Id("auth-phone-input"));
            driver.FindElement(By.Id("auth-phone-input")).Click();

            WriteNumbers(phoneNumber);

            driver.FindElement(By.TagName("ic-arrow-right")).Click();

            WaitShowElement(By.ClassName("tt-card-container"));
            var code = GenerateCode(phoneNumber);
            Thread.Sleep(3000);
            WriteNumbers(code);

            Thread.Sleep(1000);
            // todo check auth
        }

        [TestMethod]
        public void TestPhoneAuthorizeInvalidCode()
        {
            driver.Navigate().GoToUrl("https://testqa.torrow.net");

            WaitShowElement(By.Id("auth-phone-input"));
            driver.FindElement(By.Id("auth-phone-input")).Click();

            WriteNumbers(phoneNumber);

            driver.FindElement(By.TagName("ic-arrow-right")).Click();

            WaitShowElement(By.ClassName("tt-card-container"));
            var invalidCode = GenerateInvalidCode(phoneNumber);
            Thread.Sleep(3000);

            WriteNumbers(invalidCode);
            Thread.Sleep(2000);
            ClearNumbers(CountCodeNumbersForPhone);

            WriteNumbers(invalidCode);
            Thread.Sleep(2000);
            ClearNumbers(CountCodeNumbersForPhone);

            WriteNumbers(invalidCode);

            Thread.Sleep(2000);
            WaitShowElement(By.ClassName("alert-wrapper"));

            Thread.Sleep(1000);
            // todo check auth
        }

        [TestMethod]
        public void TestPositiveEmailAuthorize()
        {
            driver.Navigate().GoToUrl("https://testqa.torrow.net");

            WaitShowElement(By.ClassName("auth-phone-content__tomail-button"));
            driver.FindElement(By.ClassName("auth-phone-content__tomail-button")).Click();

            WaitShowElement(By.ClassName("auth-mail-content__input-tag"));
            driver.FindElement(By.ClassName("auth-mail-content__input-tag")).SendKeys(email);
            driver.FindElement(By.ClassName("auth-mail-content__input-tag")).SendKeys(Keys.Enter);

            WaitShowElement(By.ClassName("auth-mail-code-content__card"));
            Thread.Sleep(3000);
            WriteNumbers(emailCode);

            Thread.Sleep(3000);
            // todo check auth
        }

        [TestMethod]
        public void TestEmailAuthorizeInvalidCode()
        {
            driver.Navigate().GoToUrl("https://testqa.torrow.net");

            WaitShowElement(By.ClassName("auth-phone-content__tomail-button"));
            driver.FindElement(By.ClassName("auth-phone-content__tomail-button")).Click();

            WaitShowElement(By.ClassName("auth-mail-content__input-tag"));
            driver.FindElement(By.ClassName("auth-mail-content__input-tag")).SendKeys(email);
            driver.FindElement(By.ClassName("auth-mail-content__input-tag")).SendKeys(Keys.Enter);

            WaitShowElement(By.ClassName("auth-mail-code-content__card"));
            WriteNumbers(emailInvalidCode);
            Thread.Sleep(2000);
            ClearNumbers(CountCodeNumbersForEmail);

            WriteNumbers(emailInvalidCode);
            Thread.Sleep(2000);
            ClearNumbers(CountCodeNumbersForEmail);

            WriteNumbers(emailInvalidCode);
            Thread.Sleep(2000);

            WaitShowElement(By.ClassName("alert-wrapper"));

            Thread.Sleep(3000);
        }

        private static int[] GenerateCode(int[] phone)
        {
            int[] code = new int[4];

            for (int i = 0; i < 4; i++)
            {
                code[i] = phone[phone.Length - 1 - i];
            }

            return code;
        }

        private static int[] GenerateInvalidCode(int[] phone)
        {
            int[] code = new int[4];

            for (int i = 0; i < 4; i++)
            {
                var number = phone[phone.Length - 1 - i];
                if (number < 9)
                {
                    number = number + 1;
                }
                else
                {
                    number = 0;
                }
                code[i] = number;
            }

            return code;
        }

        private static void ClearNumbers(int count)
        {
            WaitShowElement(By.TagName("tt-key-button"));
            var buttons = driver.FindElements(By.TagName("tt-key-button"));

            for (int i = 0; i < count; i++)
            {
                buttons[CleanButtonIndex].Click();
            }
        }

        private static void WriteNumbers(int[] numbers)
        {
            WaitShowElement(By.TagName("tt-key-button"));
            var buttons = driver.FindElements(By.TagName("tt-key-button"));

            for (int i = 0; i < numbers.Length; i++)
            {
                var num = numbers[i];
                if (num == 0)
                {
                    buttons[ZeroButtonIndex].Click();
                }
                else
                {
                    buttons[num - 1].Click();
                }
            }
        }

        [TestCleanup]
        public void TearDown()
        {
            driver.Quit();
        }
    }
}
