﻿using fido2NetLib;
using Newtonsoft.Json;
using System;
using System.IO;
using Xunit;

namespace fido2_net_lib.Test
{
    // todo: Create tests and name Facts and json files better.
    public class UnitTest1
    {
        [Fact]
        public void TestParsing()
        {
            var jsonPost = JsonConvert.DeserializeObject<AuthenticatorAttestationRawResponse>(File.ReadAllText("./json1.json"));
            var options = JsonConvert.DeserializeObject<OptionsResponse>(File.ReadAllText("./options1.json"));

            Assert.NotNull(jsonPost);

            var fido2 = new fido2NetLib.Fido2NetLib(new Fido2NetLib.Configuration());
            var o = AuthenticatorAttestationResponse.Parse(jsonPost);
            o.Verify(options, "https://localhost:44329");
        }
    }
}