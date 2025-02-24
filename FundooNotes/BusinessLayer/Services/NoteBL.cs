﻿using BusinessLayer.Interfaces;
using CommonLayer;
using RepositoryLayer.Entity;
using RepositoryLayer.Interface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Services
{
    public class NoteBL : INoteBL
    {
        INoteRL noteRL;
        public NoteBL(INoteRL noteRL)
        {
            this.noteRL = noteRL;
        }

        public async Task AddNote(int userId, NotePostModel notePostModel)
        {
            try
            {
                await this.noteRL.AddNote(userId, notePostModel);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Note> UpdateNote(int userId, int noteId, NoteUpdateModel noteUpdateModel)
        {
            try
            {
                return await this.noteRL.UpdateNote(userId, noteId, noteUpdateModel);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public  Task DeleteNote(int noteId, int userId)
        {
            try
            {
                return this.noteRL.DeleteNote(noteId, userId);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task ArchiveNote(int userId, int noteId)
        {
            try
            {
                await this.noteRL.ArchiveNote(userId, noteId);
            }
            catch (Exception)
            {

                throw;
            }
        } 

        public async Task ChangeColour(int userId, int noteId, string colour)
        {
            try
            {
                await this.noteRL.ChangeColour(userId, noteId, colour);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Note> PinNote(int userId, int noteId )
        {
            try
            {
                return await this.noteRL.PinNote(userId, noteId);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Note> TrashNote(int userId, int noteId)
        {
            try
            {
                return await this.noteRL.TrashNote(userId, noteId);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<Note>> GetAllNote(int userId)
        {
            try
            {
                return await this.noteRL.GetAllNote(userId);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task ReminderNote(int userId, int noteId, DateTime ReminderDate)
        {
            try
            {
                 await this.noteRL.ReminderNote(userId, noteId, ReminderDate);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
