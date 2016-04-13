This repository has CI set up at AppVeyor.  You can view the latest build details at https://ci.appveyor.com/project/codethug/simpletv.

[![Build status](https://ci.appveyor.com/api/projects/status/pfwj1adp499h6yco?svg=true)](https://ci.appveyor.com/project/codethug/simpletv)

To get AppVeyor to push a new release to GitHub, replacing `1.2.3` with the actual version number:

```
git tag -a 1.2.3 -m "Release Notes Here"
git push origin master --tags
```

This will trigger AppVeyor to build, and if the build succeeds and all tests pass, AppVeyor will create a 
release on GitHub and upload the build artifacts to the release on GitHub.
