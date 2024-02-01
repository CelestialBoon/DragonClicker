# Unity URP Template
---
***This is a template project for Unity URP (Universal Render Pipeline) with Unity-specific git configurations.***

## Getting Started

This template has been configured to be used with **[Git LFS](https://git-lfs.github.com)**. So, make sure to install **Git LFS** to get the full benefit.

After cloning the repo, make sure to update all packages in the project to the latest.

To enable pre-commit validations, put the ***pre-commit*** file into ***<your_repo>/.git/hooks*** directory. To disable it, remove the file from the directory.

## Info

The pre-commit hook script for Unity enforces the GitHub file size limit and ensures meta files stay in sync,
as well as check that every folder & file marked to be ignored in ***.gitignore*** has an entry for its meta file to be ignored as well.

## References

Official ***.gitignore*** for Unity: https://github.com/github/gitignore/blob/e5323759e387ba347a9d50f8b0ddd16502eb71d4/Unity.gitignore

Unity Github Config from NYU Game Center: https://github.com/NYUGameCenter/Unity-Git-Config

Unity Forum: https://forum.unity.com/threads/life-with-unity-git-lfs-a-rant-and-a-call-for-help.653191/#post-7966920
