﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Wpf.Ui.Common;
using Wpf.Ui.Common.Interfaces;
using Wpf.Ui.Mvvm.Contracts;
using XHS.IService.XHS;
using XHS.Models.XHS.ApiOutputModel.OtherInfo;
using XHS.Spider.Helpers;
using XHS.Spider.Services;

namespace XHS.Spider.ViewModels
{
    public partial class UserProfileViewModel : ObservableObject, INavigationAware
    {
        #region 变量
        public static readonly string BaseUrl = "https://www.xiaohongshu.com/user/profile/";
        private string inputText;
        public string InputText
        {
            get => inputText;
            set => SetProperty(ref inputText, value);
        }
        private readonly ISnackbarService _snackbarService;
        private readonly IXhsSpiderService _xhsSpiderService;

        private BitmapImage _headImage = new BitmapImage();

        public BitmapImage HeadImage {
            get=> _headImage;
            set=>SetProperty(ref _headImage, value);
        }
        private BitmapImage _sexImage =new BitmapImage();
        public BitmapImage SexImage {
            get => _sexImage;
            set=>SetProperty(ref _sexImage, value);
        }

        private Tag info = new Tag();
        public Tag Info { 
            get=> info;
            set => SetProperty(ref info, value);
        }

        private Tag location=new Tag();
        public Tag Location { 
            get=> location;
            set=>SetProperty(ref location, value);
        }
        private Tag profession=new Tag();

        public Tag Profession { 
            get=> profession; set => SetProperty(ref profession, value);
        }
        /// <summary>
        /// 关注
        /// </summary>
        private Interaction follows=new Interaction();

        public Interaction Follows {
            get => follows; set => SetProperty(ref follows, value);
        }

        /// <summary>
        /// 粉丝
        /// </summary>
        private Interaction fans = new Interaction();

        public Interaction Fans { 
            get=> fans; set => SetProperty(ref fans, value);
        }
        /// <summary>
        /// 获赞与收藏
        /// </summary>
        private Interaction interaction = new Interaction();

        public Interaction Interaction {
            get => interaction; set => SetProperty(ref interaction, value);
        }
        private OtherInfoModel _userInfo = new OtherInfoModel();
        public OtherInfoModel UserInfo
        {
            get => _userInfo;
            set => SetProperty(ref _userInfo, value);
        }
        public UserProfileViewModel(ISnackbarService snackbarService, IXhsSpiderService xhsSpiderService)
        {
            _snackbarService = snackbarService;
            _xhsSpiderService = xhsSpiderService;
        }


        // 输入确认事件
        private ICommand inputCommand;
        public ICommand InputCommand
        {
            get => inputCommand ?? (inputCommand = new Wpf.Ui.Common.RelayCommand(ExecuteInitData));
            set => inputCommand = value;
        }
        #endregion

        /// <summary>
        /// 处理输入事件
        /// </summary>
        public void ExecuteInitData()
        {
            if (!string.IsNullOrEmpty(InputText))
            {
                if (InputText.Contains("user/profile/"))
                {
                   var id= SearchService.GetId(inputText,BaseUrl);
                    if (string.IsNullOrEmpty(id)) {
                        return;
                    }
                    else
                    {
                        var apiResult= _xhsSpiderService.GetOtherInfo(id);
                        if (apiResult != null && apiResult.Success)
                        {
                            UserInfo = apiResult.Data;
                            var info= UserInfo.Tags.FirstOrDefault(e=>e.TagType== "info");
                            if(info!=null)
                                Info= info;
                            var location = UserInfo.Tags.FirstOrDefault(e => e.TagType == "location");
                            if (location != null)
                                Location = location;
                            var profession = UserInfo.Tags.FirstOrDefault(e => e.TagType == "profession");
                            if (profession != null)
                                Profession = profession;
                            var follows = UserInfo.Interactions.FirstOrDefault(e => e.Type == "follows");
                            if(follows!=null)
                                Follows = follows;
                            var fans = UserInfo.Interactions.FirstOrDefault(e => e.Type == "fans");
                            if (fans != null)
                                Fans = fans;
                            var interaction = UserInfo.Interactions.FirstOrDefault(e => e.Type == "interaction");
                            if (interaction != null)
                                Interaction =interaction;

                            var baseInfo = UserInfo.BasicInfo;
                            App.PropertyChangeAsync(new Action(() =>
                            {
                                if (baseInfo != null && !string.IsNullOrEmpty(baseInfo.Imageb))
                                {
                                    //TODO:处理url？号后参数
                                    var imageUrl= baseInfo.Imageb.Split('?')[0];
                                    var headImage = FileHelper.UrlToBitmapImage(imageUrl);
                                    HeadImage = headImage;
                                }
                                if (!string.IsNullOrEmpty(info?.Icon))
                                {
                                    var sex= FileHelper.UrlToBitmapImage(info.Icon);
                                    SexImage=sex;
                                }
                            }));
                           
                            var nodes = _xhsSpiderService.GetAllUserNode(id);

                        }
                        else
                        {
                            _snackbarService.Show("异常", apiResult?.Msg, SymbolRegular.ErrorCircle12, ControlAppearance.Danger);
                        }
                    }
                }
                else
                {
                    _snackbarService.Show("提示", "当前Url不符合所属模块搜索要求", SymbolRegular.ErrorCircle12, ControlAppearance.Danger);
                }
            }
            //TODO:
        }

        public void OnNavigatedFrom()
        {
        }

        public void OnNavigatedTo()
        {

        }
    }
}