# CustomFileUploader
Following on from https://blueimp.github.io/jQuery-File-Upload/, I have modified the project slightly to allow a user pass in a string as an identifier and all uploaded files are prefixed with the identified passed in.

If not identifier is passed in, the system automatically generates and id i.e. DateTime.Now.ToString("ddMMyyyyhhmmss") and this is used to prefix all uploaded documents.
