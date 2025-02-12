using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace losertron4000
{
    public class ExpressionButton : ImageButton
    {
        public static readonly BindableProperty UriProperty =
           BindableProperty.Create(
               nameof(Uri),                // Property name
               typeof(string),                    // Property type
               typeof(DokiExpression),           // Declaring type
               string.Empty);

        public static readonly BindableProperty CategoryProperty =
           BindableProperty.Create(
               nameof(Category),                // Property name
               typeof(string),                    // Property type
               typeof(DokiExpression),           // Declaring type
               string.Empty);


        public string Uri
        {
            get => (string)GetValue(UriProperty);
            set => SetValue(UriProperty, value);
        }

        public string Category
        {
            get => (string)GetValue(CategoryProperty);
            set => SetValue(CategoryProperty, value);
        }     
    }



    public class DokiExpression : INotifyPropertyChanged
    {
        private string trueUri;
        public string TrueUri
        {
            get => trueUri;
            set
            {
                if (trueUri != value)
                {
                    trueUri = value;
                    OnPropertyChanged(nameof(TrueUri));
                }
            }
        }

        private Path uri;
        public Path Uri
        {
            get => uri;
            set
            {
                if (uri != value)
                {
                    uri = value;
                    OnPropertyChanged(nameof(Uri));
                }
            }
        }

        private string category;
        public string Category
        {
            get => category;
            set
            {
                if (category != value)
                {
                    category = value;
                    OnPropertyChanged(nameof(Category));
                }
            }
        }

        private Color backgroundColor;
        public Color BackgroundColor
        {
            get => backgroundColor;
            set
            {
                if (backgroundColor != value)
                {
                    backgroundColor = value;
                    OnPropertyChanged(nameof(BackgroundColor));
                }
            }
        }

        private bool enabled = true;
        public bool IsEnabled
        {
            get => enabled;
            set
            {
                if (enabled != value)
                {
                    enabled = value;
                    OnPropertyChanged(nameof(IsEnabled));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool Equals(DokiExpression other)
        {
            return TrueUri.Equals(other.TrueUri) && Uri.ToString().Equals(other.Uri.ToString()) && Category.Equals(other.Category) && BackgroundColor.Equals(other.BackgroundColor);
        }

        public bool Equals(ExpressionButton other)
        {
            return Uri.ToString().Equals(other.Uri.ToString()) && Category.Equals(other.Category) && BackgroundColor.Equals(other.BackgroundColor);
        }

        public DokiExpression(Path imageUri)
        {
            Uri = imageUri;
            TrueUri = imageUri.ToString();
            Category = string.Empty;
            BackgroundColor = Colors.Transparent;
        }

        public DokiExpression()
        {
            Uri = new Path("");
            TrueUri = Uri;
            Category = "";
            BackgroundColor = Colors.Transparent;
            IsEnabled = false;
        }

        public DokiExpression(ExpressionButton ex)
        {
            Uri = new Path(ex.Uri);
            TrueUri = ex.Uri;
            Category = ex.Category;
            BackgroundColor = Colors.Transparent;
            IsEnabled = ex.IsEnabled;
        }
    }
}
