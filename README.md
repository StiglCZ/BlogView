# BlogView
A blog viewing webserver

Running
-----------------
Like any other dotnet package. Run in the same directory as the .conf and .html file.
`dotnet run` Will just run the program, without any special options
`dotnet build` Builds the executable of the program, and if you want a single file executable you can simply do `dotnet build -c Release --self-contained true /p:PublishSingleFile=true -r <YOUR_PLATFORM>`


Uploading Posts
-----------------
Simply upload to the `RootDirectory/blogs`, or whatever you have set in your config file. The posts will automatically appear on the website next time you refresh(in case they don't, make sure the website is not cached by the browser)


Configuring
-----------------
You can simply edit the blog.conf like any other .conf file. In the file, you can edit:

`Port`  - The port webserver will run on. If you are a non-privileged user, make sure its above 1024
`Title` - The title of the website above all posts
`Color` - Background color of the entire website(not the raw files)
`RootDirectory` - Parent directory of `BlogDirectory`
`BlogDirectory` - Directory where all your blogs are located
`FileExtension` - File extension of all your blogs. If empty, missing extension will result in 404 error

