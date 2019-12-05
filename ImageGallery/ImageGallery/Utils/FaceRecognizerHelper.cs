using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ImageGallery.Utils
{
    public class FaceRecognizerHelper
    {
        private static string _apiKey;
        private static string _endpoint;
        private static IFaceClient _faceClient;
        private static FaceRecognizerHelper _faceApiHelper;

        public static string FaceApiKey
        {
            get { return _apiKey; }
            set
            {
                if (_apiKey != value)
                {
                    _apiKey = value;
                    InitializeService();
                }
            }
        }
        public static string FaceApiEndpoint
        {
            get { return _endpoint; }
            set
            {
                if (_endpoint != value)
                {
                    if (Uri.IsWellFormedUriString(value, UriKind.Absolute))
                    {
                        _endpoint = value;
                        _faceClient.BaseUri = new Uri(value);
                    }
                    else
                    {
                        throw new Exception("Invalid edpoint Uri");
                    }
                    InitializeService();
                }
            }
        }

        private FaceRecognizerHelper() { }

        public static FaceRecognizerHelper InitializeService()
        {
            if (_faceApiHelper == null)
            {
                _faceApiHelper = new FaceRecognizerHelper();
                _faceClient = new FaceClient(new ApiKeyServiceClientCredentials(_apiKey), new System.Net.Http.DelegatingHandler[] { });
            }
            return _faceApiHelper;

        }

        public async Task<bool> TainImagesAsync(string username, string[] imagePaths)
        {
            Guid personGroupId;
            try
            {
                var personGroupList = await _faceClient.PersonGroup.ListAsync();
                var personGrp = personGroupList.FirstOrDefault(s => s.Name == "users");
                if (personGrp==null)
                {
                    personGroupId = Guid.NewGuid();
                    await _faceClient.PersonGroup.CreateAsync(personGroupId.ToString(), "users");
                }
                else
                {
                    Guid.TryParse(personGrp.PersonGroupId, out personGroupId);
                }

                var personGroup = await _faceClient.PersonGroup.GetAsync(personGroupId.ToString());

                var person = await _faceClient.PersonGroupPerson.CreateAsync(personGroupId.ToString(), username);
                foreach (var path in imagePaths)
                {
                    var imageStream = System.IO.File.OpenRead(path);
                    await _faceClient.PersonGroupPerson.AddPersonFaceFromStreamAsync(personGroup.PersonGroupId, person.PersonId, imageStream);
                    //await _faceClient.PersonGroupPerson.AddPersonFaceFromUrlAsync(personGroup.PersonGroupId, person.PersonId, path);            
                }

                await _faceClient.PersonGroup.TrainAsync(personGroupId.ToString());
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public async Task<string> DetectUserAsync(Stream imageStream)
        {
            PersonGroup personGroup;

            try
            {
                var personGroups = await _faceClient.PersonGroup.ListAsync();
                if (personGroups.Count > 0)
                {
                    personGroup = personGroups.FirstOrDefault(s => s.Name == "users");
                    if (personGroup != null)
                    {
                        var faces = await _faceClient.Face.DetectWithStreamAsync(imageStream);
                        if (faces.Count > 0)
                        {
                            var faceIds = faces.Select(s => s.FaceId.Value).ToList();
                            var results = await _faceClient.Face.IdentifyAsync(personGroup.PersonGroupId, faceIds);
                            if (results.Count > 0)
                            {
                                var personId = results[0].Candidates[0].PersonId;
                                var person = await _faceClient.PersonGroupPerson.GetAsync(personGroup.PersonGroupId, personId);
                                return person.Name;
                            }
                            else
                            {
                                throw new Exception("Could not identify the person");
                            }
                        }
                        else
                        {
                            throw new Exception("No faces detected");
                        }
                    }
                    else
                    {
                        throw new Exception("No person group created for login users");
                    }
                }
                else
                {
                    throw new Exception("Not yet created any Person groups");
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void DeleteAllPersonGroupsAsync()
        {
            var personGroupList = _faceClient.PersonGroup.ListAsync().Result;
            foreach (var grp in personGroupList)
            {
                _faceClient.PersonGroup.DeleteAsync(grp.PersonGroupId);
            }

        }
    }
}