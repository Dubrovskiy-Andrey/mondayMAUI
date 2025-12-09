using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiApp4.DTO;
using MauiApp4.Model;
using MauiApp4.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp4.ViewModel
{
    public partial class ContactsViewModel : ObservableObject
    {
        private readonly IApiService _apiService;

        [ObservableProperty]
        private ObservableCollection<ContactDto> _contacts = new ObservableCollection<ContactDto>();

        [ObservableProperty]
        private ContactDto _selectedContact;

        [ObservableProperty]
        private string _searchText;

        [ObservableProperty]
        private bool _isRefreshing;

        [ObservableProperty]
        private bool _isModalVisible;

        [ObservableProperty]
        private ContactDto _editingContact;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _modelTitle;

        public ContactsViewModel()
        {
            _apiService = new ApiService();
            _ = LoadContactsAsync();
        }

        private async Task LoadContactsAsync()
        {
            await LoadContacts();
        }

        [RelayCommand]
        private async Task LoadContacts()
        {
            try
            {
                IsBusy = true;
                var contacts = await _apiService.GetContactsAsync(SearchText);

                Contacts.Clear();
                foreach (var contact in contacts)
                {
                    Contacts.Add(contact);
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка",
                    $"Ошибка загрузки данных: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void AddContact()
        {
            EditingContact = new ContactDto();
            ModelTitle = "Новый контакт";
            IsModalVisible = true;
        }

        [RelayCommand]
        private void EditContact(ContactDto contact)
        {
            if (contact != null)
            {
                EditingContact = new ContactDto
                {
                    Id = contact.Id,
                    FirstName = contact.FirstName,
                    LastName = contact.LastName,
                    Phone = contact.Phone,
                    Email = contact.Email,
                    Address = contact.Address
                };
                ModelTitle = "Редактирование контакта";
                IsModalVisible = true;
            }
        }

        [RelayCommand]
        private async Task DeleteContact(ContactDto contact)
        {
            if (contact == null) return;

            bool answer = await Application.Current.MainPage.DisplayAlert(
                "Удаление контакта",
                $"Вы уверены, что хотите удалить контакт {contact.FullName}?",
                "Да", "Нет");

            if (answer)
            {
                try
                {
                    await _apiService.DeleteContactAsync(contact.Id);

                    Contacts.Remove(contact);

                    await Application.Current.MainPage.DisplayAlert(
                        "Успешно", "Контакт удален", "OK");
                }
                catch (Exception ex)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Ошибка", $"Не удалось удалить контакт: {ex.Message}", "OK");
                }
            }
        }

        [RelayCommand]
        private async Task SaveContact()
        {
            if (EditingContact == null) return;

            try
            {
                IsBusy = true;

                if (EditingContact.Id == 0) 
                {
                    var createDto = new CreateContactDto
                    {
                        FirstName = EditingContact.FirstName,
                        LastName = EditingContact.LastName,
                        Phone = EditingContact.Phone,
                        Email = EditingContact.Email,
                        Address = EditingContact.Address
                    };

                    var newContact = await _apiService.CreateContactAsync(createDto);
                    Contacts.Add(newContact);
                }
                else 
                {
                    var updateDto = new UpdateContactDto
                    {
                        FirstName = EditingContact.FirstName,
                        LastName = EditingContact.LastName,
                        Phone = EditingContact.Phone,
                        Email = EditingContact.Email,
                        Address = EditingContact.Address
                    };

                    await _apiService.UpdateContactAsync(EditingContact.Id, updateDto);

                    var existingContact = Contacts.FirstOrDefault(c => c.Id == EditingContact.Id);
                    if (existingContact != null)
                    {
                        existingContact.FirstName = EditingContact.FirstName;
                        existingContact.LastName = EditingContact.LastName;
                        existingContact.Phone = EditingContact.Phone;
                        existingContact.Email = EditingContact.Email;
                        existingContact.Address = EditingContact.Address;
                    }
                }

                CloseModal();
                await LoadContacts();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Ошибка", $"Не удалось сохранить контакт: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task RefreshContacts()
        {
            IsRefreshing = true;
            await LoadContacts();
            IsRefreshing = false;
        }

        [RelayCommand]
        private void SearchContact()
        {
            _ = LoadContacts();
        }

        [RelayCommand]
        private void CloseModal()
        {
            IsModalVisible = false;
            EditingContact = null;
        }
    }
}