using System.Windows.Input;
using Prism.Mvvm;
using Prism.Commands;
using ImageSlideshow.Views;

namespace ImageSlideshow.ViewModels {
    class RunForm : BindableBase {
        public ICommand StartForm { get; private set; }
        public RunForm() {
            StartForm = new DelegateCommand(ShowMethod);
        }
        private void ShowMethod() {
            Form objPopupwindow = new Form();
            objPopupwindow.ShowDialog();
        }
    }
}
