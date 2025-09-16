using Microsoft.Extensions.Logging;
using NiesPro.ServiceAdmin.ViewModels;
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace NiesPro.ServiceAdmin
{
    /// <summary>
    /// Professional WPF Main Window with MVVM pattern and dependency injection
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ILogger<MainWindow> _logger;
        private readonly MainViewModel _viewModel;

        /// <summary>
        /// Initializes the main window with dependency injection
        /// </summary>
        /// <param name="viewModel">Main view model</param>
        /// <param name="logger">Logger instance</param>
        public MainWindow(MainViewModel viewModel, ILogger<MainWindow> logger)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            try
            {
                InitializeComponent();
                
                // Set the DataContext to the injected ViewModel
                DataContext = _viewModel;

                // Initialize time display timer (removed for modern interface)
                _logger.LogInformation("NiesPro Professional Service Administration initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing MainWindow");
                MessageBox.Show(
                    $"Erreur lors de l'initialisation de l'interface: {ex.Message}",
                    "Erreur d'Initialisation",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handles window loaded event
        /// </summary>
        protected override async void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            
            try
            {
                _logger.LogInformation("NiesPro Professional window source initialized, starting services refresh");
                
                // Start initial services refresh
                if (_viewModel.RefreshCommand.CanExecute(null))
                {
                    _viewModel.RefreshCommand.Execute(null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during window source initialization");
            }
        }

        /// <summary>
        /// Handles window closing event
        /// </summary>
        protected override void OnClosing(CancelEventArgs e)
        {
            try
            {
                _logger.LogInformation("NiesPro Service Administration closing initiated");

                // Stop update timer in ViewModel
                _viewModel?.StopUpdates();

                _logger.LogInformation("NiesPro Service Administration closed gracefully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during window closing");
            }
            finally
            {
                base.OnClosing(e);
            }
        }

        /// <summary>
        /// Handles unhandled exceptions in the window
        /// </summary>
        private void Window_Unhandled(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                var exception = e.ExceptionObject as Exception;
                _logger.LogCritical(exception, "Unhandled exception in MainWindow");

                var result = MessageBox.Show(
                    $"Une erreur critique s'est produite:\n\n{exception?.Message}\n\nVoulez-vous continuer?",
                    "Erreur Critique",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Error);

                if (result == MessageBoxResult.No)
                {
                    Application.Current.Shutdown(1);
                }
            }
            catch
            {
                // Last resort - try to shutdown gracefully
                Application.Current.Shutdown(1);
            }
        }

        /// <summary>
        /// Handles dispatcher unhandled exceptions
        /// </summary>
        private void Dispatcher_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                _logger.LogError(e.Exception, "Unhandled dispatcher exception in MainWindow");

                // Mark as handled to prevent application crash
                e.Handled = true;

                // Show user-friendly error message
                MessageBox.Show(
                    $"Une erreur s'est produite: {e.Exception.Message}",
                    "Erreur",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Error handling dispatcher exception");
            }
        }

        /// <summary>
        /// Override to handle potential UI threading issues
        /// </summary>
        public override void EndInit()
        {
            try
            {
                base.EndInit();
                
                // Subscribe to global error handling
                Dispatcher.UnhandledException += Dispatcher_UnhandledException;
                AppDomain.CurrentDomain.UnhandledException += Window_Unhandled;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during EndInit");
            }
        }

        #region Custom Window Controls

        /// <summary>
        /// Handles title bar mouse down for window dragging
        /// </summary>
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    this.DragMove();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during window drag");
            }
        }

        /// <summary>
        /// Handles minimize button click
        /// </summary>
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.WindowState = WindowState.Minimized;
                _logger.LogDebug("Window minimized");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error minimizing window");
            }
        }

        /// <summary>
        /// Handles close button click
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _logger.LogInformation("Closing NiesPro Service Administration");
                this.Close();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing window");
                Application.Current.Shutdown();
            }
        }

        #endregion
    }
}