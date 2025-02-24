﻿using CommonLayer;
using Experimental.System.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RepositoryLayer.Entity;
using RepositoryLayer.FundooContext;
using RepositoryLayer.Interface;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace RepositoryLayer.Services
{
    public class UserRL : IUserRL
    {
        //instance of  FundooContext Class
        FundooDBContext fundooDBContext;

        //Constructor
        public IConfiguration configuration { get; }

        public UserRL(FundooDBContext fundooDBContext, IConfiguration configuration)
        {
            this.fundooDBContext = fundooDBContext;
            this.configuration = configuration;
        }

        //Method to register User Details.
        public void AddUser(UserPostModel user)
        {
            //string passwordToEncript = string.Empty;
            try
            {
                string passwordToEncript = string.Empty;
                User user1 = new User();

                user1.userID = new User().userID;
                user1.firstName = user.firstName;
                user1.lastName = user.lastName;
                user1.email = user.email;
                //user1.password = EncryptPassword(user.password);
                passwordToEncript = EncodePasswordToBase64(user.password);
                user1.password = passwordToEncript;

                user1.registeredDate = DateTime.Now;
                fundooDBContext.Add(user1);
                fundooDBContext.SaveChanges();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string EncodePasswordToBase64(string password)
        {
            try
            {
                byte[] encData_byte = new byte[password.Length];
                encData_byte = System.Text.Encoding.UTF8.GetBytes(password);
                string encodedData = Convert.ToBase64String(encData_byte);
                return encodedData;
            }
            catch (Exception ex)
            {

                throw new Exception("Error in base64Encode" + ex.Message);
            }
        }

        public string DecodeFrom64(string encodedData)
        {
            System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding();
            System.Text.Decoder utf8Decode = encoder.GetDecoder();
            byte[] todecode_byte = Convert.FromBase64String(encodedData);
            int charCount = utf8Decode.GetCharCount(todecode_byte, 0, todecode_byte.Length);
            char[] decoded_char = new char[charCount];
            utf8Decode.GetChars(todecode_byte, 0, todecode_byte.Length, decoded_char, 0);
            string result = new string(decoded_char);
            return result;
        }

        public string LoginUser(string email, string password)
        {
            try
            {

                var AllRecords = fundooDBContext.Users.ToList();
                var existingRecord = AllRecords.Where(x => x.email == email).FirstOrDefault();

                if (existingRecord != null)
                {
                    var decriptedPassword = DecodeFrom64(existingRecord.password);
                    bool conditionCheck = decriptedPassword == password ? true : false;
                    if (conditionCheck == false)
                    {
                        return "Invalid credentials";
                    }
                    else
                    {
                        return GetJWTToken(existingRecord.email, existingRecord.userID);
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }



        //public string EncryptPassword(string password)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(password))
        //        {
        //            return null;
        //        }
        //        else
        //        {
        //            byte[] b = Encoding.ASCII.GetBytes(password);
        //            string encrypted = Convert.ToBase64String(b);
        //            return encrypted;
        //        }
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }
        //}

        //public static string DecryptedPassword(string encryptedPassword)
        //{
        //    byte[] b;
        //    string decrypted;
        //    try
        //    {
        //        if (string.IsNullOrEmpty(encryptedPassword))
        //        {
        //            return null;
        //        }
        //        else
        //        {
        //            b = Convert.FromBase64String(encryptedPassword);
        //            decrypted = Encoding.ASCII.GetString(b);
        //            return decrypted;
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //        throw ex;
        //    }
        //}





        //public string LoginUser(string email, string password)
        //{
        //    try
        //    {
        //        //Linq query matches given input in database and returns that entry from the database.
        //        var result = fundooDBContext.Users.FirstOrDefault(u => u.email == email && u.password == password);
        //        if (result == null)
        //        {
        //            return null;
        //        }

        //        //Calling Jwt Token Creation method and returning token.
        //        return GetJWTToken(email, result.userID);

        //    }
        //    catch (Exception ex)
        //    {

        //        throw ex;
        //    }
        //}

        //Implementing Jwt Token For Login using Email and Id
        private static string GetJWTToken(string email, int userID)
        {

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenKey = Encoding.ASCII.GetBytes("THIS_IS_MY_KEY_TO_GENERATE_TOKEN");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("email", email),
                    new Claim("userID",userID.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(1),

                SigningCredentials =
                new SigningCredentials(
                    new SymmetricSecurityKey(tokenKey),
                    SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        // User ForgotPasssword
        public bool ForgotPassword(string email)
        {
            try
            {
                var userdata = fundooDBContext.Users.FirstOrDefault(u => u.email == email);
                if (userdata == null)
                {
                    return false;
                }
                MessageQueue queue;
                // Add Message to Queue

                if (MessageQueue.Exists(@".\Private$\FundooQueue"))
                {
                    queue = new MessageQueue(@".\Private$\FundooQueue");
                }
                else
                {
                    queue = MessageQueue.Create(@".\Private$\FundooQueue");
                }

                Message MyMessage = new Message();
                MyMessage.Formatter = new BinaryMessageFormatter();
                MyMessage.Body = GetJWTToken(email, userdata.userID);
                MyMessage.Label = "Forgot Password Email";
                queue.Send(MyMessage);

                Message msg = queue.Receive();
                msg.Formatter = new BinaryMessageFormatter();
                EmailService.SendMail(email, msg.Body.ToString());
                queue.ReceiveCompleted += new ReceiveCompletedEventHandler(msmqQueue_ReciveCompleted);

                queue.BeginReceive();
                queue.Close();
                return true;

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        private void msmqQueue_ReciveCompleted(object sender, ReceiveCompletedEventArgs e)
        {
            try
            {
                {
                    MessageQueue queue = (MessageQueue)sender;
                    Message msg = queue.EndReceive(e.AsyncResult);
                    EmailService.SendMail(e.Message.ToString(), GenerateToken(e.Message.ToString()));
                    queue.BeginReceive();
                }

            }
            catch (MessageQueueException ex)
            {

                if (ex.MessageQueueErrorCode ==
                   MessageQueueErrorCode.AccessDenied)
                {
                    Console.WriteLine("Access is denied. " +
                        "Queue might be a system queue.");
                }
                // Handle other sources of MessageQueueException.
            }
        }

        private string GenerateToken(string email)
        {
            if (email == null)
            {
                return null;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenKey = Encoding.ASCII.GetBytes("THIS_IS_MY_KEY_TO_GENERATE_TOKEN");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("Email", email)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials =
                new SigningCredentials(
                    new SymmetricSecurityKey(tokenKey),
                    SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public bool ResetPassword(ResetPassword resetPassword, string email)
        {
            try
            {
                var user = fundooDBContext.Users.FirstOrDefault(u => u.email == email);
                if (resetPassword.NewPassword.Equals(resetPassword.ConfirmPassword))
                {
                    user.password = EncodePasswordToBase64(resetPassword.NewPassword);
                    fundooDBContext.SaveChanges();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public List<User> GetAllUsers()
        {
            try
            {
                var result = fundooDBContext.Users.ToList();
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
