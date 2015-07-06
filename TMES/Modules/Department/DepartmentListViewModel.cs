﻿using AECSCore;
using DAL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Windows.Input;
using System.Collections.ObjectModel;

namespace TMES.ViewModel
{
    public class DepartmentListViewModel : ModuleViewModel
    {
        //:: TODO : Replace ModuleViewModel to specialized
        //:: container class that will be common for all list/edit objects
        private ModuleViewModel _parent;
        public ModuleViewModel Parent
        {
            get
            {
                return _parent;
            }
            set
            {
                _parent = value;
            }
        }

        private UserContext _context;
        public UserContext Context
        {
            get
            {
                if(_context == null)
                {
                    _context = new UserContext();
                }
                return _context;
            }
            set
            {
                _context = value;
            }
        }

        private Boolean _isEditable;
        public Boolean IsEditable
        {
            get
            {
                return _isEditable;
            } 
            set
            {
                _isEditable = value;
            }
        }

        private ObservableCollection<Department> _items;
        public ObservableCollection<Department> Items
        {
            get
            {
                if(_items == null)
                {
                    _items = new ObservableCollection<Department>();
                }
                return _items;
            }
            set
            {
                _items = value;
                OnPropertyChanged("Items");
            }
        }

        private Department _selectedItem;

        public Department SelectedItem
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                _selectedItem = value;
                if(Parent is DepartmentManagerViewModel)
                 {
                    (Parent as DepartmentManagerViewModel).SelectedItem = value;
                }
                OnPropertyChanged("SelectedItem");
            }
        }

        public DepartmentListViewModel()
        {
            Context.Departments.Load();
            this.Items = Context.Departments.Local;
            IsEditable = true;
        }

        public DepartmentListViewModel(ModuleViewModel Parent)
        {
            this.Parent = Parent;
            Context.Departments.Load();
            this.Items = Context.Departments.Local;
            IsEditable = true;  
        }

        public DepartmentListViewModel(Department Department)
        {
            Context.Departments.Load();
            var AllDepartments = Context.Departments.Local;
            this.Items = AllDepartments;
        }

        #region Commands

        private RelayCommand _createCommand;
        public ICommand CreateCommand
        {
            get
            {
                if (_createCommand == null)
                {
                    _createCommand = new RelayCommand(param => this.Create(), null);
                }
                return _createCommand;
            }
        }

        private RelayCommand _deleteCommand;
        public ICommand DeleteCommand
        {
            get
            {
                if (_deleteCommand == null)
                {
                    _deleteCommand = new RelayCommand(param => this.Delete(), null);
                }
                return _deleteCommand;
            }
        }

        #endregion Commands

        #region Methods

        private void Create()
        {
            var CreatedItem = new Department()
            {
                Name = "Цех #", 
                ShortName = "Ц#"
            };
            Context.Departments.Add(CreatedItem);
            UserContext.ChangedMade();
  
            Context.SaveChanges();
        }

        private void Delete()
        {
            (Items as ObservableCollection<Department>).Remove(SelectedItem);
            Context.SaveChanges();
        }

        #endregion Methods
    }
}
