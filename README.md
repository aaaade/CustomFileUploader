# CustomFileUploader
Following on from https://blueimp.github.io/jQuery-File-Upload/, I have modified the project slightly to allow a user pass in a string as an identifier and all uploaded files are prefixed with the identifier passed in.

If no identifier is passed in, the system automatically generates an id i.e. `DateTime.Now.ToString("ddMMyyyyhhmmss")` and this is used to prefix all uploaded files.

Its current usage is to load an iframe with the Url of your site while passing the id using `[SiteAddress]/FileUpload/id=[Identifier]`. The Index view of this applicaition contains numerous partial views, one of which has an iframe on it and the response from `[SiteAddress]/FileUpload/id=[Identifier]` is set as the src attribute of the iframe.

Also, the system also allows you load up files associated with specific identifiers. It can be called using `[SiteAddress]/FileUpload/Edit/id=[Identifier]`
