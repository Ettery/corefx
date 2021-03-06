// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;
using Xunit;

namespace System.Net.Primitives.Functional.Tests
{
    public static partial class NetworkCredentialTest
    {
        public static IEnumerable<object[]> SecurePassword_TestData()
        {
            yield return new object[] { "password", AsSecureString("password") };
            yield return new object[] { "SecurePassword", AsSecureString("SecurePassword") };
            yield return new object[] { "", AsSecureString("") };
            yield return new object[] { null, AsSecureString(null) };
        }

        public static IEnumerable<object[]> Password_RoundTestData()
        {
            yield return new object[] { "password", "planPassword" };
            yield return new object[] { "SecurePassword", "justOnePassword" };
            yield return new object[] { "", "OneMoreTest" };
            yield return new object[] { null, null };
        }

        [Fact]
        public static void Ctor_SecureString_Test()
        {
            string expectedUser = "UserName";

            using (SecureString expectedSecurePassword = AsSecureString("password"))
            {
                NetworkCredential nc = new NetworkCredential(expectedUser, expectedSecurePassword);
                Assert.Equal(expectedUser, nc.UserName);
                Assert.True(expectedSecurePassword.CompareSecureString(nc.SecurePassword));
            }
        }

        [Fact]
        public static void Ctor_SecureStringDomain_Test()
        {
            string expectedUser = "UserName";
            string expectedDomain = "thisDomain";

            using (SecureString expectedSecurePassword = AsSecureString("password"))
            {
                NetworkCredential nc = new NetworkCredential(expectedUser, expectedSecurePassword, expectedDomain);
                Assert.Equal(expectedUser, nc.UserName);
                Assert.Equal(expectedDomain, nc.Domain);
                Assert.True(expectedSecurePassword.CompareSecureString(nc.SecurePassword));
            }
        }

        [Theory]
        [MemberData(nameof(SecurePassword_TestData))]
        public static void SecurePasswordSetGet_Test(string password, SecureString expectedPassword)
        {
            NetworkCredential nc = new NetworkCredential();
            using (SecureString securePassword = AsSecureString(password))
            {
                nc.SecurePassword = securePassword;
                Assert.True(expectedPassword.CompareSecureString(nc.SecurePassword));
            }
        }

        [Theory]
        [MemberData(nameof(Password_RoundTestData))]
        public static void SecurePassword_Password_RoundData_Test(string expectedSecurePassword, string expectedPassword)
        {
            NetworkCredential nc = new NetworkCredential();
            using (SecureString securePassword = AsSecureString(expectedSecurePassword))
            {
                nc.SecurePassword = securePassword;
                Assert.Equal(expectedSecurePassword ?? string.Empty, nc.Password);

                nc.Password = expectedPassword;
                Assert.True(AsSecureString(expectedPassword).CompareSecureString(nc.SecurePassword));
            }
        }

        private static bool CompareSecureString(this SecureString s1, SecureString s2)
        {
            return s1 != null && s2 != null && AsString(s1) == AsString(s2);
        }

        private static string AsString(SecureString sstr)
        {
            if (sstr.Length == 0)
            {
                return string.Empty;
            }

            IntPtr ptr = IntPtr.Zero;
            string result = string.Empty;
            try
            {
                ptr = Marshal.SecureStringToGlobalAllocUnicode(sstr);
                result = Marshal.PtrToStringUni(ptr);
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                {
                    Marshal.ZeroFreeGlobalAllocUnicode(ptr);
                }
            }
            return result;
        }

        private static SecureString AsSecureString(string str)
        {
            SecureString secureString = new SecureString();

            if (string.IsNullOrEmpty(str))
            {
                return secureString;
            }

            foreach (char ch in str)
            {
                secureString.AppendChar(ch);
            }

            return secureString;
        }
    }
}
