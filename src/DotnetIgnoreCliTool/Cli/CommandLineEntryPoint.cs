﻿using DotnetIgnoreCliTool.Cli.Args;
using DotnetIgnoreCliTool.Cli.FIles;
using DotnetIgnoreCliTool.Cli.Output;
using DotnetIgnoreCliTool.Github.Models;
using DotnetIgnoreCliTool.Github.Services;
using PowerArgs;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetIgnoreCliTool.Cli
{
    public class CommandLineEntryPoint
    {
        private readonly IOutput _output;
        private readonly IGitignoreGithubService _githubService;
        private readonly IGitignoreFileWriter _gitignoreFileWriter;

        public CommandLineEntryPoint()
        {
            _output = new ConsoleOutput();
            _githubService = new GitignoreGithubService();
            _gitignoreFileWriter = new GitignoreFileWritter();
        }

        [HelpHook, ArgShortcut("-h"), ArgDescription("shows help")]
        public bool Help { get; set; }

        [ArgActionMethod]
        [ArgDescription("list all available .gitignore files")]
        public async Task List(ListGitignoreArgs args)
        {
            IReadOnlyList<string> gitignoreFilesNames = await _githubService.GetAllIgnoreFilesNames();

            if (args.Short)
            {
                gitignoreFilesNames = gitignoreFilesNames
                    .Select(fileName => fileName.Replace(".gitignore", ""))
                    .ToImmutableList();
            }

            foreach (var gitignoreFileName in gitignoreFilesNames)
            {
                _output.WriteLine($"- {gitignoreFileName}");
            }
        }

        [ArgActionMethod]
        [ArgDescription("downloads selected gitignore file")]
        public async Task Get(GetGitignoreArgs getGitignoreArgs)
        {
            GitignoreFile gitgnoreFile = await _githubService.GetIgnoreFile(getGitignoreArgs.Name);

            if (GitignoreFile.Empty == gitgnoreFile)
            {
                throw new ArgException($"Name {getGitignoreArgs.Name} is not correct .gitignore file name");
            }

            await _gitignoreFileWriter.WriteToFileAsync(getGitignoreArgs.Destination, gitgnoreFile.Content);
        }

        //TODO Merge action - merges given gitignore files into one file
    }
}