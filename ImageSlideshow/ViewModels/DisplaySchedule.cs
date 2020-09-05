using System.Windows.Input;
using Prism.Mvvm;
using Prism.Commands;
using ImageSlideshow.Views;

namespace ImageSlideshow.ViewModels {
    class DisplaySchedule : BindableBase {
        public ICommand ShowCommand { get; private set; }
        public DisplaySchedule() {
            ShowCommand = new DelegateCommand(ShowMethod);
        }
        public void ShowMethod() {
            Schedule objPopupwindow = new Schedule();
            objPopupwindow.ShowDialog();
        }
    }
}
