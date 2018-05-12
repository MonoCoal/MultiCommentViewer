﻿using System;
using System.Collections.Generic;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;
using SitePlugin;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Common;

namespace MultiCommentViewer
{
    public class ConnectionContext
    {
        public ConnectionName ConnectionName { get; set; }
        public ICommentProvider CommentProvider { get; set; }
        public ISiteContext SiteContext { get; set; }
    }
    public class SelectedSiteChangedEventArgs : EventArgs
    {
        public ConnectionName ConnectionName { get; set; }
        public ConnectionContext OldValue { get; set; }
        public ConnectionContext NewValue { get; set; }
    }
    public class ConnectionViewModel : ViewModelBase
    {
        public ConnectionName ConnectionName => _connectionName;
        public string Name
        {
            get { return _connectionName.Name; }
            set { _connectionName.Name = value; }
        }
        public bool IsSelected { get; set; }
        public ObservableCollection<SiteViewModel> Sites { get; }
        public ObservableCollection<BrowserViewModel> Browsers { get; }
        private SiteViewModel _selectedSite;
        private ICommentProvider _commentProvider = null;
        private readonly ConnectionName _connectionName;
        public ICommand ConnectCommand { get; }
        public ICommand DisconnectCommand { get; }
        public event EventHandler<SelectedSiteChangedEventArgs> SelectedSiteChanged;

        private ConnectionContext _beforeContext;
        private ConnectionContext _currentContext;
        public SiteViewModel SelectedSite
        {
            get { return _selectedSite; }
            set
            {
                if (_selectedSite == value)
                    return;
                //一番最初は_commentProviderはnull
                var before = _commentProvider;
                if (before != null)
                {
                    Debug.Assert(before.CanConnect, "接続中に変更はできない");
                    before.CanConnectChanged -= CommentProvider_CanConnectChanged;
                    before.CanDisconnectChanged -= CommentProvider_CanDisconnectChanged;
                    before.CommentReceived -= CommentProvider_CommentReceived;
                    before.InitialCommentsReceived -= CommentProvider_InitialCommentsReceived;
                    before.MetadataUpdated -= CommentProvider_MetadataUpdated;
                }
                _selectedSite = value;
                var next = _commentProvider = _selectedSite.Site.CreateCommentProvider();
                next.CanConnectChanged += CommentProvider_CanConnectChanged;
                next.CanDisconnectChanged += CommentProvider_CanDisconnectChanged;
                next.CommentReceived += CommentProvider_CommentReceived;
                next.InitialCommentsReceived += CommentProvider_InitialCommentsReceived;
                next.MetadataUpdated += CommentProvider_MetadataUpdated;

                System.Windows.Controls.UserControl commentPanel;
                try
                {
                    commentPanel = _selectedSite.Site.GetCommentPostPanel(next);
                }
                catch (Exception ex)
                {
                    _logger.LogException(ex);
                    commentPanel = null;
                }
                CommentPostPanel = commentPanel;

                _beforeContext = _currentContext;
                _currentContext = new ConnectionContext
                {
                     ConnectionName = this.ConnectionName,
                      CommentProvider = next,
                       SiteContext = _selectedSite.Site,
                };
                RaisePropertyChanged();
                SelectedSiteChanged?.Invoke(this, new SelectedSiteChangedEventArgs
                {
                    ConnectionName = this.ConnectionName,
                    OldValue = _beforeContext,
                    NewValue = _currentContext
                });
            }
        }

        private System.Windows.Controls.UserControl _commentPostPanel;
        public System.Windows.Controls.UserControl CommentPostPanel
        {
            get { return _commentPostPanel; }
            set
            {
                if (_commentPostPanel == value) return;
                _commentPostPanel = value;
                RaisePropertyChanged();
            }
        }

        private void CommentProvider_MetadataUpdated(object sender, IMetadata e)
        {
            MetadataReceived?.Invoke(this, e);//senderはConnection
        }
        public event EventHandler<RenamedEventArgs> Renamed;
        public event EventHandler<ICommentViewModel> CommentReceived;
        public event EventHandler<List<ICommentViewModel>> InitialCommentsReceived;
        public event EventHandler<IMetadata> MetadataReceived;
        private void CommentProvider_CommentReceived(object sender, ICommentViewModel e)
        {
            CommentReceived?.Invoke(this, e);
        }
        private void CommentProvider_InitialCommentsReceived(object sender, List<ICommentViewModel> e)
        {
            InitialCommentsReceived?.Invoke(this, e);
        }
        private void CommentProvider_CanDisconnectChanged(object sender, EventArgs e)
        {
            RaisePropertyChanged(nameof(CanDisconnect));
        }

        private void CommentProvider_CanConnectChanged(object sender, EventArgs e)
        {
            RaisePropertyChanged(nameof(CanConnect));
        }

        private BrowserViewModel _selectedBrowser;
        public BrowserViewModel SelectedBrowser
        {
            get { return _selectedBrowser; }
            set
            {
                _selectedBrowser = value;
                RaisePropertyChanged();
            }
        }
        public bool CanConnect
        {
            get { return _commentProvider.CanConnect; }
        }
        public bool CanDisconnect
        {
            get { return _commentProvider.CanDisconnect; }
        }
        private string _input;
        public string Input
        {
            get { return _input; }
            set
            {
                if (_input == value)
                    return;
                _input = value;

                ISiteContext sc = null;
                if (!string.IsNullOrWhiteSpace(_input))
                {
                    foreach (var site in _sites)
                    {
                        try
                        {
                            if (site.IsValidInput(_input))
                            {
                                sc = site;
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogException(ex, "", _input);
                        }
                    }
                }
                if (sc != null)
                {
                    try
                    {
                        var vm = _siteVmDict[sc];
                        SelectedSite = vm;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogException(ex);
                    }
                }
            }
        }

        public ConnectionContext GetCurrent()
        {
            var context = new ConnectionContext { ConnectionName = this.ConnectionName, SiteContext = SelectedSite.Site, CommentProvider = _commentProvider };
            return context;
        }

        private async void Connect()
        {
            try
            {
                var input = Input;
                var browser = SelectedBrowser.Browser;
                await _commentProvider.ConnectAsync(input, browser);


            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
        private void Disconnect()
        {
            try
            {
                _commentProvider.Disconnect();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                _logger.LogException(ex);
            }
        }
        string _beforeName;
        private readonly ILogger _logger;
        private readonly IEnumerable<ISiteContext> _sites;
        private readonly Dictionary<ISiteContext, SiteViewModel> _siteVmDict = new Dictionary<ISiteContext, SiteViewModel>();
        public ConnectionViewModel(ConnectionName connectionName, IEnumerable<SiteViewModel> sites, IEnumerable<BrowserViewModel> browsers, ILogger logger)
        {
            _logger = logger;
            _connectionName = connectionName ?? throw new ArgumentNullException(nameof(connectionName));
            _beforeName = _connectionName.Name;
            _connectionName.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(_connectionName.Name):
                        var newName = _connectionName.Name;
                        Renamed?.Invoke(this, new RenamedEventArgs(_beforeName, newName));
                        _beforeName = newName;
                        RaisePropertyChanged(nameof(Name));
                        break;
                }
            };

            if (sites == null)
            {
                throw new ArgumentNullException(nameof(sites));
            }
            _sites = sites.Select(m => m.Site);
            Sites = new ObservableCollection<SiteViewModel>();
            foreach (var siteVm in sites)
            {
                _siteVmDict.Add(siteVm.Site, siteVm);
                Sites.Add(siteVm);
            }
            //Sites = new ObservableCollection<SiteViewModel>(sites);
            if (Sites.Count > 0)
            {
                SelectedSite = Sites[0];
            }

            Browsers = new ObservableCollection<BrowserViewModel>(browsers);
            if (Browsers.Count > 0)
            {
                SelectedBrowser = Browsers[0];
            }
            ConnectCommand = new RelayCommand(Connect);
            DisconnectCommand = new RelayCommand(Disconnect);
        }
    }
    public class RenamedEventArgs : EventArgs
    {
        public string NewValue { get; }
        public string OldValue { get; }
        public RenamedEventArgs(string oldValue, string newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}