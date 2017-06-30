#!/bin/bash
set -e
echo 'Running'

pushd `dirname $0` > /dev/null
SCRIPT_PATH=`pwd`
popd > /dev/null

remoteRepos=(
 "https://github.com/Clancey/SimpleAuth.git"
 "https://github.com/Clancey/SimpleTables.git"
 "https://github.com/Clancey/SimpleDatabase.git" 
 "https://github.com/Clancey/FlyoutNavigation.git"
 "https://github.com/Clancey/MonoTouch.Dialog.git"
 "https://github.com/paulcbetts/ModernHttpClient.git"
 "https://github.com/Clancey/YoutubeExtractor.git"
 "https://github.com/Clancey/taglib-sharp.git"
 "https://github.com/Clancey/lastfm-sharp.git")
echo $remoteRepos
#Go back a folder and clone here
cd ..
for gitRepo in  "${remoteRepos[@]}"
do
	echo $gitRepo
	gitPath=$(echo ${gitRepo}|cut -d'/' -f5)
	localRepoDir=$(echo $gitPath | sed 's/.git//g')
  	if [ -d $localRepoDir ]; then 	
  		cd $localRepoDir		
		echo -e "Running in $localRepoDir: \n git pull"
		git pull
		cd ..
	else
		cloneCmd="git clone "$gitRepo	
		echo -e "Running: \n$ $cloneCmd \n"	
		git clone "$gitRepo"
	fi
done

cd FlyoutNavigation
git checkout dialog
cd ..

cd SimpleAuth
mono --runtime=v4.0 $SCRIPT_PATH/tools/NuGet/nuget.exe restore src/SimpleAuth.sln
