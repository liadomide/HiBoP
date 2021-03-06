﻿using UnityEngine;
using UnityEngine.UI;
using Tools.Unity.Lists;
using System.Linq;

namespace HBP.UI
{
    public class PatientItem : ActionnableItem<Data.Patient>
    {
        #region Properties
        [SerializeField] Text m_NameText;
        [SerializeField] Text m_PlaceText;
        [SerializeField] Text m_DateText;

        [SerializeField] Text m_MeshText;
        [SerializeField] Text m_MRIText;
        [SerializeField] Text m_SitesText;
        [SerializeField] Text m_TagsText;

        [SerializeField] State m_ErrorState;

        public override Data.Patient Object
        {
            get
            {
                return base.Object;
            }
            set
            {
                base.Object = value;
                m_NameText.text = value.Name;
                m_PlaceText.text = value.Place;
                m_DateText.text = value.Date.ToString();

                m_MeshText.SetIEnumerableFieldInItem("Meshes", from mesh in value.Meshes where mesh.WasUsable select mesh.Name, m_ErrorState);
                m_MRIText.SetIEnumerableFieldInItem("MRIs", from MRI in value.MRIs where MRI.WasUsable select MRI.Name, m_ErrorState);
                m_SitesText.SetIEnumerableFieldInItem("Sites", from site in value.Sites select site.Name, m_ErrorState);
                m_TagsText.SetIEnumerableFieldInItem("Tags", from tag in value.Tags select tag.Tag.Name, m_ErrorState);
            }
        }
        #endregion
    }
}