// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Manage.v1;

namespace SampleApp
{
    class Program
    {
        private const string MEMBER_BY_EMAIL = "MEMBER_TO_DELETE_BY_EMAIL";

        static async Task Main(string[] args)
        {
            // Initialize Library with default logging
            // Normal logging is "Info" level
            Library.Initialize();

            // use the client factory with a API Key set with the "DEEPGRAM_API_KEY" environment variable
            var deepgramClient = ClientFactory.CreateManageClient();

            // get projects
            var projectResp = await deepgramClient.GetProjects();
            if (projectResp == null)
            {
                Console.WriteLine("ListProjects failed.");
                Environment.Exit(1);
            }

            string myId = null;
            string myName = null;
            foreach (var project in projectResp.Projects)
            {
                myId = project.ProjectId;
                myName = project.Name;
                break;
            }
            Console.WriteLine($"\n\n{projectResp}\n\n");

            // list members
            string memberId = null;
            var listResp = await deepgramClient.GetMembers(myId);
            if (listResp == null)
            {
                Console.WriteLine("\n\nNo members found\n\n");
            }
            else
            {
                Console.WriteLine($"\n\n{listResp}\n\n");

                foreach (var member in listResp.Members)
                {
                    if (member.Email == MEMBER_BY_EMAIL)
                    {
                        memberId = member.MemberId;
                    }
                }
            }

            if (memberId == null)
            {
                Console.WriteLine("This example requires a member who is already a member with email in the value of \"MEMBER_BY_EMAIL\".");
                Console.WriteLine("This is required to exercise the UpdateMemberScope function.");
                Console.WriteLine("In the absence of this, this example will exit early.");
                Environment.Exit(1);
            }

            // get member scope
            var memberResp = await deepgramClient.GetMemberScopes(myId, memberId);
            if (memberResp == null)
            {
                Console.WriteLine("\n\nNo scopes found\n\n");
                Environment.Exit(1);
            }
            Console.WriteLine($"\n\n{memberResp}\n\n");

            // update scope
            var scopeUpdate = new MemberScopeSchema()
            {
                Scope = "admin"
            };
            var updateResp = await deepgramClient.UpdateMemberScope(myId, memberId, scopeUpdate);
            Console.WriteLine($"\n\n{updateResp}\n\n");

            // get member scope
            memberResp = await deepgramClient.GetMemberScopes(myId, memberId);
            if (memberResp == null)
            {
                Console.WriteLine("\n\nNo scopes found\n\n");
                Environment.Exit(1);
            }
            Console.WriteLine($"\n\n{memberResp}\n\n");

            // update scope
            scopeUpdate = new MemberScopeSchema()
            {
                Scope = "member",
            };
            updateResp = await deepgramClient.UpdateMemberScope(myId, memberId, scopeUpdate);
            Console.WriteLine($"\n\n{updateResp}\n\n");

            // get member scope
            memberResp = await deepgramClient.GetMemberScopes(myId, memberId);
            if (memberResp == null)
            {
                Console.WriteLine("\n\nNo scopes found\n\n");
                Environment.Exit(1);
            }
            Console.WriteLine($"\n\n{memberResp}\n\n");


            Console.WriteLine("\n\nPress any key to exit.");
            Console.ReadKey();

            // Teardown Library
            Library.Terminate();
        }
    }
}