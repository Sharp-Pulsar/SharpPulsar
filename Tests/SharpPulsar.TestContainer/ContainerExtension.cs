using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Docker.DotNet.Models;
using DotNet.Testcontainers.Containers;

namespace SharpPulsar.TestContainer
{
    public static class ContainerExtension
    {
        //https://github.com/dotnet/Docker.DotNet/blob/386bfd6d84e94205e75102cd838147683d42d89c/src/Docker.DotNet/Endpoints/ContainerOperations.cs#L350-L379
        //https://github.com/GodelTech/CodeReview.Orchestrator/blob/378dd201395eb7423c55e9c4e0d43407dad2a999/src/CodeReview.Orchestrator/Services/ContainerService.cs#L167-L187
        //https://github.com/GodelTech/CodeReview.Orchestrator/blob/0c6617581f7a1cf78da24674c8e20f1dcdd7cfaa/src/CodeReview.Orchestrator/Services/DockerVolumeExporter.cs#L27-L50
        //https://github.com/GodelTech/CodeReview.Orchestrator/blob/0c6617581f7a1cf78da24674c8e20f1dcdd7cfaa/src/CodeReview.Orchestrator/Activities/ExportFolderActivity.cs#L30-L50
        //https://github.com/GodelTech/CodeReview.Orchestrator/blob/c070b8960089a183a2bcaa0b3726b5a39c6e9c65/src/CodeReview.Orchestrator/Services/TarArchiveService.cs#L39-L69
        public static async Task ExportFilesFromContainerAsync( this TestcontainersContainer container, string containerId, string containerPath, Stream outStream)
        {
            if (outStream == null)
                throw new ArgumentNullException(nameof(outStream));
            if (string.IsNullOrWhiteSpace(containerId))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(containerId));
            if (string.IsNullOrWhiteSpace(containerPath))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(containerPath));

            using var client = container. _dockerClientFactory.Create();

            var result = await client.Containers.GetArchiveFromContainerAsync(
                containerId,
                new GetArchiveFromContainerParameters
                {
                    Path = containerPath
                },
                false);

            await result.Stream.CopyToAsync(outStream);
        }
    }
}
