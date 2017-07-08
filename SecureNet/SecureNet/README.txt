App.xaml equivalent to CSS 
	refer to this file to see the styles I've done, x:key=_____ is like Css class
	You call it like:
		<Button Style="{DynamicResource menuBtn}"></Button>
		menuBtn is the class
	refer to Home.xaml for examples if needed

MainWindow.xaml = Master page, main layout (header, content, footer) is done here
	Imagine it's like 3 sheets of paper put together, Header paper, Content paper, Footer paper
	Our layout is that content paper is changed but not header/footer paper

Home.xaml is the page displayed at master page on set up

Creating new pages
	When you create new pages, Add New Page.
	When you open it, you will see the first few lines has the <Page: etc etc> code.
	Ensure the following is changed/added in on the .xaml page:
		d:DesignHeight="550" d:DesignWidth="1050"
		  ShowsNavigationUI="False"
		  Title="Home"
	Modify the .xaml.cs page to look like the following. Add in the 'Style=' etc line:
		public Home()
			{
				InitializeComponent();
				Style = (Style)FindResource(typeof(Page));

			}
	Refer to Home.xaml for the example.