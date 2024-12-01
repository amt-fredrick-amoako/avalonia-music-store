using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows.Input;
using Avalonia.MusicStore.Models;
using ReactiveUI;

namespace Avalonia.MusicStore.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            ShowDialog = new Interaction<MusicStoreViewModel, AlbumViewModel?>();

            BuyMusicCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var store = new MusicStoreViewModel();

                var result = await ShowDialog.Handle(store);
                if (result != null)
                {
                    Albums.Add(result);
                    await result.SaveToDiskAsync();
                }
            });
            RxApp.MainThreadScheduler.Schedule(LoadAlbums);
        }

        public ICommand BuyMusicCommand { get; }

        public Interaction<MusicStoreViewModel, AlbumViewModel?> ShowDialog { get; }

        public ObservableCollection<AlbumViewModel> Albums { get; } = [];

        private async void LoadAlbums()
        {
            var albums = (await Album.LoadCachedAsync()).Select(a => new AlbumViewModel(a));
            var albumViewModels = albums as AlbumViewModel[] ?? albums.ToArray();
            foreach (var album in albumViewModels)
            {
                Albums.Add(album);
            }
            
            foreach (var album in albumViewModels.ToList())
            {
                await album.LoadCover();
            }
        }
    }
}