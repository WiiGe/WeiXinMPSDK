﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using Senparc.Weixin.Entities;

namespace Senparc.WeixinTests.Entities
{
    [TestClass]
    public class SenparcWeixinSettingTests
    {
        [TestMethod]
        public void InitTest()
        {
            var senparcWeixinSetting = new SenparcWeixinSetting(true);
            Assert.AreEqual(true, senparcWeixinSetting.IsDebug);

            var defaultItem = senparcWeixinSetting.Items["Default"];
            Assert.IsNotNull(defaultItem);

            var appId = Guid.NewGuid().ToString("n");
            var secret = Guid.NewGuid().ToString("n");
            senparcWeixinSetting.WeixinAppId = appId;
            senparcWeixinSetting.WeixinAppSecret = secret;

            //defaultItem 和 SenparcWeixinSetting 的基类是同一个对象
            Assert.AreEqual(appId, defaultItem.WeixinAppId);
            Assert.AreEqual(secret, defaultItem.WeixinAppSecret);

            //添加新项目
            var newItem = senparcWeixinSetting.Items["New"];
            var newAppId = "Senparc" + Guid.NewGuid().ToString("n");
            newItem.WeixinAppId = newAppId;
            Assert.AreEqual(newAppId, senparcWeixinSetting.Items["New"].WeixinAppId);

        }

        [TestMethod]
        public void PrivateKeyTest()
        {
            var senparcWeixinSetting = new SenparcWeixinSetting(true);
            var privateKey = "~/apiclient_key.pem";
            senparcWeixinSetting.TenPayV3_PrivateKey = privateKey;
            Console.WriteLine(senparcWeixinSetting.TenPayV3_PrivateKey);
            Assert.IsTrue(senparcWeixinSetting.TenPayV3_PrivateKey.Length > 100);

            var exceptResult = "MIIEvwIBADANBgkqhkiG9w0BAQEFAASCBKkwggSlAgEAAoIBAQDee2pWWXyO6nhEdf5nj/MvQj0/tDNBrwCT+JK38ZY8xIzNbIj7J2+oBPimkFpjtETRr1YoV4BDSxt0/3E0Rdb6+gTGhdYavXM7aYA7qY/EA1eI81Zp32Hfsvnl07tFybA1W/m2CRlJQIlmXBYurd9F0Os3MSZjRBoWyrd9ISgsKcco+wdeJkxGW12MoYNVZutAlXZBoNDisjZLc2koCi7fDspsjezOTUcBnbKtTohMB7lYaFORMItELXCQ6rHwIE8zO3sYzHIkM3MR8ilSJ59CnLh0R53uqkQJ7AGVgbv97XsUMjnPiWT4B9xzvR4KGWnMs0tBr/J/0mfZlRqqOLE5AgMBAAECggEAL9XjUDufX28kervP/l5iEDgyyR6qoqXI/wfELA6imeA80EFTJYUeKccf21hQRv28ikUjxjrCFjXw6l/97BpUFdRp8HFYTpmLTCvr6WgUxDVfvc9sNglUlu95caPrsR6jZ2WmNDCSokBhCoQkNNcnmXBJEq3brh43ac0eVKYraAsQ4G+CHhdzmbB8XLc+lg2qekqXOLiNRNCm9FAJ1669EiJrK2ry9rWFZvP9d5DuyqL9djX0ff+DqUvvQiV3usQlmeJdRVAgajWJYyVec0g442wY5xKfGZ0K+l8wp2ne5nxz7F6XBONWpTrpO8EUVN0ix5nnzBvQKSNDaIeFncEkMQKBgQD1JxSP+Ky3I1dGQpXpMXC3WOYiJyZqv5BmH3A+XKErEmEXwa0CfXnxhnS0yA8Ipp4SFuu3ynynhw/2/Ca1fXd3TRG0Xg0oVNSbN8Oscrj8GnCu2yCM/RM/5sJFyc/sH+hExyApSfqmnUINLytMBtJDRKOXEeC1d/z1ET8c1mkJhQKBgQDoU4nfxtMsN1gPkHhdvvH1xcPB/mUknjzCi/336YDfmABtgZ4kcK7HVDhDW7+JnB1m66xptCHfCWiIkWQwxDK19h1wChigxbqY1aTkvlGMQQaNQYVHxPMPjqhl1WxCt20IW72dRpmruPGMggZmhBiX5vmIOIl87VHLvCo0c9FdJQKBgQCwCjkszWCRPhKMxIHL65HKR08ylTR0EU2K1+aNEY02VcNdANnQ4POxKWEi9Eo/Zw45ZTYtS31J+6XOMPFHAGrKQ5CEGcmO/aOSNnAPpG4LspzaI0Zzl8O77mPxI2NoZt0ujmMc4x/XhzOILigENx3D6kUi1VasWRZPkOvmNF1G1QKBgQCNTWHqDM+bcP3KWaAbxGr9hI8Pil6R6vwhh2usQQT0+UopUFCS8UYcTgj6Tu8sDxuC4Yw3rrO4p8xAY82AK5R8P3igEEPyZNCc7DQiO+71UwddGqCpigwbRjT92tTBrzZNgx7MbYhBfXbMcrjZ2TXsDbtvMpPMu7qoI4W36UlJUQKBgQCzRtp0q5bq9mMic01F17Hhq5xnGExs3EMA18USh4p0Xh2eX0klKI2CskPPr7uRUiuTTg0o4dZ+W91hIQw3WVTFWfod1KfijjV2RaFqE9iW1/iCarTC3NOCtPIr8iJZmPRbqQH8Ja4GOsTza50y5eo+YRmwhWcLFbA7/WTcvNZ5qw==";

            Assert.AreEqual(exceptResult, senparcWeixinSetting.TenPayV3_PrivateKey);
        }
    }
}
