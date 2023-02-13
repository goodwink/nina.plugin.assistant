﻿using Assistant.NINAPlugin.Database.Schema;
using NINA.Core.Utility;
using System.ComponentModel;
using System.Windows.Input;

namespace Assistant.NINAPlugin.Controls.AssistantManager {

    public class TargetViewVM : BaseINPC {

        private AssistantManagerVM managerVM;
        private TargetProxy targetProxy;

        public TargetProxy TargetProxy {
            get => targetProxy;
            set {
                targetProxy = value;
                RaisePropertyChanged(nameof(TargetProxy));
            }
        }

        public TargetViewVM(AssistantManagerVM managerVM, Target target) {
            this.managerVM = managerVM;
            TargetProxy = new TargetProxy(target);

            EditCommand = new RelayCommand(Edit);
            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);

        }

        private void TargetProxy_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e?.PropertyName != nameof(TargetProxy.Proxy)) {
                TargetChanged = true;
            }
            else {
                RaisePropertyChanged(nameof(TargetProxy));
            }
        }

        private bool showTargetEditView = false;
        public bool ShowTargetEditView {
            get => showTargetEditView;
            set {
                showTargetEditView = value;
                RaisePropertyChanged(nameof(ShowTargetEditView));
            }
        }

        private bool targetChanged = false;
        public bool TargetChanged {
            get => targetChanged;
            set {
                targetChanged = value;
                RaisePropertyChanged(nameof(TargetChanged));
            }
        }

        public ICommand EditCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        private void Edit(object obj) {
            TargetProxy.PropertyChanged += TargetProxy_PropertyChanged;
            managerVM.SetEditMode(true);
            ShowTargetEditView = true;
            TargetChanged = false;
        }

        private void Save(object obj) {
            managerVM.SaveTarget(TargetProxy.Proxy);
            TargetProxy.OnSave();
            TargetProxy.PropertyChanged -= TargetProxy_PropertyChanged;
            ShowTargetEditView = false;
            managerVM.SetEditMode(false);
        }

        private void Cancel(object obj) {
            TargetProxy.OnCancel();
            TargetProxy.PropertyChanged -= TargetProxy_PropertyChanged;
            ShowTargetEditView = false;
            managerVM.SetEditMode(false);
        }

    }
}
