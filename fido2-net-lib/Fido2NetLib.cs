﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace fido2NetLib
{
    /// <summary>
    /// Public API for parsing and veriyfing FIDO2 attestation & assertion responses.
    /// </summary>
    public class Fido2NetLib
    {
        public class Configuration
        {
            public uint Timeout { get; set; } = 60000;
            public int ChallengeSize { get; set; } = 64;
            public string ServerDomain { get; set; }
            public string ServerName { get; set; }
            public string ServerIcon { get; set; }
            public string Origin { get; set; }
        }

        private Configuration Config { get; }

        private RandomNumberGenerator _crypto;

        public Fido2NetLib(Configuration config)
        {
            Config = config;
            _crypto = RandomNumberGenerator.Create();

        }

        /// <summary>
        /// Returns CredentialCreateOptions including a challenge to be sent to the browser/authr to create new credentials
        /// </summary>
        /// <returns></returns>
        public CredentialCreateOptions RequestNewCredential(User user, string requestedAttesstation)
        {
            // https://w3c.github.io/webauthn/#dictdef-publickeycredentialcreationoptions
            // challenge.rp
            // challenge.user
            // challenge.excludeCredentials
            // challenge.authenticatorSelection
            // challenge.attestation
            // challenge.extensions

            // note: I have no idea if this crypto is ok...
            var challenge = new byte[Config.ChallengeSize];
            _crypto.GetBytes(challenge);
            
            var options = CredentialCreateOptions.Create(challenge, Config);
            options.User = user;
            options.Attestation = requestedAttesstation;

            return options;
        }

        /// <summary>
        /// Verifies the response from the browser/authr after creating new credentials
        /// </summary>
        /// <param name="attestionResponse"></param>
        /// <param name="origChallenge"></param>
        /// <returns></returns>
        public CredentialMakeResult MakeNewCredential(AuthenticatorAttestationRawResponse attestionResponse, CredentialCreateOptions origChallenge)
        {
            var parsedResponse = AuthenticatorAttestationResponse.Parse(attestionResponse);
            var res = parsedResponse.Verify(origChallenge, Config.Origin);


            var pk = BitConverter.ToString(res.PublicKey);
            var cid = BitConverter.ToString(res.CredentialId);

            // todo: Set Errormessage etc.
            return new CredentialMakeResult { Status = "ok", ErrorMessage = "", Result = res };
        }

        /// <summary>
        /// Returns AssertionOptions including a challenge to the browser/authr to assert existing credentials and authenticate a user.
        /// </summary>
        /// <returns></returns>
        public AssertionOptions GetAssertion(User user, List<PublicKeyCredentialDescriptor> allowedCredentials)
        {

            var challenge = new byte[Config.ChallengeSize];
            _crypto.GetBytes(challenge);

            var options = AssertionOptions.Create(challenge, allowedCredentials, Config);

            return options;


        }

        /// <summary>
        /// Verifies the assertion response from the browser/authr to assert existing credentials and authenticate a user.
        /// </summary>
        /// <returns></returns>
        public bool MakeAssertion(AuthenticatorAssertionRawResponse assertionResponse, AssertionOptions origOptions, byte[] existingPublicKey)
        {
            var parsedResponse = AuthenticatorAssertionResponse.Parse(assertionResponse);
            parsedResponse.Verify(origOptions, Config.Origin, 0, false, existingPublicKey);

            return true;
        }

        /// <summary>
        /// Result of parsing and verifying attestation. Used to transport Public Key back to RP
        /// </summary>
        public class CredentialMakeResult
        {
            public string Status { get; set; }
            public string ErrorMessage { get; set; }
            public AttestationVerificationData Result { get; internal set; }

            // todo: add debuginfo?
        }
    }
}
