using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using StreamCore.Config;
using StreamCore.SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace AlphaTwitch.UI
{
    public class TwitchViewersListViewController : BSMLResourceViewController
    {
        public override string ResourceName => "AlphaTwitch.Views.twitchviewerslist.bsml";

        public Action<Viewer> viewerClicked;

        [UIComponent("list")]
        public CustomListTableData viewerListTableData;

        protected override void DidActivate(bool firstActivation, ActivationType type)
        {
            base.DidActivate(firstActivation, type);
            if (firstActivation)
            {
                rectTransform.anchorMin = new Vector3(0.5f, 0, 0);
                rectTransform.anchorMax = new Vector3(0.5f, 1, 0);
                rectTransform.sizeDelta = new Vector3(70, 0, 0);
            }

            if (type == ActivationType.AddedToHierarchy)
            {
                viewerListTableData.tableView.ClearSelection();
                SetData();
            }
        }

        List<Viewer> cellInfo = new List<Viewer>();

        [UIAction("viewer-click")]
        private void ClickedRow(TableView table, int row)
        {
            viewerClicked?.Invoke(cellInfo[row]);

        }

        public void SetData()
        {
            StartCoroutine(SetDataEnumerator());
        }

        private IEnumerator SetDataEnumerator()
        {
            yield return new WaitForSecondsRealtime(.1f);
            using (UnityWebRequest www = UnityWebRequest.Get($"https://tmi.twitch.tv/group/user/{TwitchLoginConfig.Instance.TwitchChannelName}/chatters"))
            {
                www.SetRequestHeader("Client-ID", "gbrndadd5hgnm9inrdsw8xpx5w9xa9");
                yield return www.SendWebRequest();

                JSONNode response = JSON.Parse(www.downloadHandler.text);
                List<string> names = new List<string>();

                var chl = response["chatters"];

                foreach (JSONNode name in chl["broadcaster"].AsArray)
                    names.Add(name);
                foreach (JSONNode name in chl["vips"].AsArray)
                    names.Add(name);
                foreach (JSONNode name in chl["moderators"].AsArray)
                    names.Add(name);
                foreach (JSONNode name in chl["staff"].AsArray)
                    names.Add(name);
                foreach (JSONNode name in chl["admins"].AsArray)
                    names.Add(name);
                foreach (JSONNode name in chl["viewers"].AsArray)
                    names.Add(name);

                List<Viewer> unsorted = new List<Viewer>();

                foreach (var name in names)
                {
                    bool foundCache = AlphaTwitchManager.Viewers.ContainsKey(name);
                    if (foundCache)
                    {
                        AlphaTwitchManager.Viewers.TryGetValue(name, out Viewer v1);
                        string roleText;
                        if (v1.Role == Role.Other)
                            roleText = "Comfy";
                        else
                            roleText = v1.Role.ToString();

                        var yeehaw = new CustomListTableData.CustomCellInfo($"<color={v1.Color}>{v1.Name}</color>", roleText, v1.Profile);
                        viewerListTableData.data.Add(yeehaw);
                        unsorted.Add(v1);
                    }
                }
                //unsorted.Sort();
                cellInfo = unsorted;
                viewerListTableData.tableView.ReloadData();
            }
        }
    }
}
