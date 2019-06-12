using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using InPlanLib;
using System.Collections;

namespace JobCreator
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        public frmMain(string _jobName)
        {
            InitializeComponent();
            jobName = _jobName;
        }

        public struct spec
        {
            public DateTime date;
            public string cust_group;
            public bool is_new;
        }

        public static string jobName;
        public static ApplicationManager app;
        public static IJobManager jobManager;
        public static IJob theJob;
        public static Array impds;
        public static IAttributesModifier attModifier;
        public static IRuleManager ruleManager;
        public static IFlowRunner flowRunner;
        public static IJobCreator jobCreator;
        private static Dictionary<string, IAttribute> uda_Dic = new Dictionary<string, IAttribute>();
        public Dictionary<string, spec> dicSpec = new Dictionary<string, spec>();

        public static bool createInPlan(string jobName,string special_spec = "")
        {
            bool res = false;
            ruleManager = app.RuleManager();
            flowRunner = ruleManager.FlowRunner("PostJobHook", "PostJobHook:PostJobMain");
            flowRunner.AddInterfaceParameter("CurrentJob", theJob);
            flowRunner.Run();
            checkStatus(flowRunner, "Job");

            string cust_code = "";
            if (jobName.Length >= 12)
            {
                cust_code = jobName.Substring(4, 3);
            }
            else
            {
                cust_code = "000";
            }

            ISpecManager specManager = app.SpecManager();
            Array specInfos = specManager.SpecInfos();

            for (int i = 0; i < specInfos.Length; i++)
            {
                ISpecInfo specInfo = (ISpecInfo)specInfos.GetValue(i);
                string spec_name = specInfo.Name();
                if (special_spec != "")
                {
                    if (spec_name == special_spec)
                    {
                        jobManager.AttachSpecToJob(theJob, specInfo);
                        break;
                    }
                }
                else if (spec_name.Contains(cust_code))
                {
                    jobManager.AttachSpecToJob(theJob, specInfo);
                    break;
                }
            }
            
            jobManager.SetJobBySpecs(theJob);

            jobManager.SaveJob(theJob);

            res = true;
            return res;
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            app = new ApplicationManager();
            
            jobManager = app.JobManager();
            theJob = jobManager.OpenJob(jobName);
            uda_Loop(theJob, "");

            //ruleManager = app.RuleManager();
            //flowRunner = ruleManager.FlowRunner("PostJobHook", "PostJobHook:Set__Customer");
            //flowRunner.AddInterfaceParameter("CurrentJob", theJob);
            //flowRunner.Run();
            //checkStatus(flowRunner, "Job");

            ICustomer customer = theJob.Customer();
            uda_Loop(customer, customer.Code());
            MessageBox.Show(customer.Code().ToString());
            //string cust_group = "华为终端";
            //string cust_group = getAttr(customer.Code() + "CUSTOMER_ENUM_").ToString();

            //ISpecManager specManager = app.SpecManager();
            //Array specInfos = specManager.SpecInfos();
            //for (int i = 0; i < specInfos.Length; i++)
            //{
            //    ISpecInfo specInfo = (ISpecInfo)specInfos.GetValue(i);
            //    if (specInfo.Description().Contains(cust_group))
            //    {
            //        string[] str =  specInfo.Description().Split('\n');
            //        spec spec = new spec();
            //        spec.date = Convert.ToDateTime(str[0]);
            //        spec.cust_group = str[1];
            //        spec.is_new = Convert.ToBoolean(str[2]);
            //        dicSpec.Add(specInfo.Name(), spec);
            //    }
            //}

            //foreach (DevExpress.XtraTab.XtraTabPage page in xtraTabControl1.TabPages)
            //{
            //    if (cust_group == page.Text && cust_group != "无")
            //    {
            //        page.PageVisible = true;
            //    }
            //}

            //if (cust_group == "华为终端")
            //{
            //    var dicSort = from objDic in dicSpec orderby objDic.Value.date descending select objDic;//料号按优先级升序排序

            //    foreach (KeyValuePair<string, spec> kvp in dicSort)
            //    {
            //        if (cmbSpecDate.Properties.Items.Contains(kvp.Value.date.ToShortDateString()) == false)
            //        {
            //            cmbSpecDate.Properties.Items.Add(kvp.Value.date.ToShortDateString());
            //        }
            //    }

            //    cmbBoard_type.Text = getAttr("BOARD_TYPE_UDA_NEW_", cmbBoard_type).ToString();
            //}
            //else
            //{
            //    createInPlan(jobName);
            //    Application.Exit();
            //}

            //string[] str = dicSpec["华为(N21_Ik级)2017-10-24"].Split('\n');
            //foreach (string s in str)
            //{
            //    MessageBox.Show(s);
            //}
 
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            attModifier = app.AttributesModifier(theJob);
            setAttr(cmbBoard_type.Text, "BOARD_TYPE_UDA_NEW_", theJob);
            attModifier.Run();
            checkStatus(attModifier, "attModifier");
            string lv = "华为(N21_I级)";
            string spec_name = "";
            if (cmbBoard_type.Text == "汽车板")
            {
                lv = "华为(N21_2级)";
            }

            if (cmbSpecDate.Text == "2017-1-1")
            {
                spec_name = lv;
            }
            else
            {
                spec_name = lv + cmbSpecDate.Text;
            }
            //MessageBox.Show(spec_name);
            createInPlan(jobName,spec_name);
            Application.Exit();
        }

        public static void uda_Loop(dynamic obj, string Keys)
        {
            Array Objattrs;
            Objattrs = obj.Attrs(true);
            foreach (IAttribute attr in Objattrs)
            {
                string key = Keys + attr.Name();
                uda_Dic.Add(key, attr);
            }
        }
        
        public static object getAttr(string key)
        {
            object a = null;
            IAttribute attr = uda_Dic[key];
            switch (attr.ExtentionContentType())
            {
                case ExtContentType.EXT_TYPE_BLOB:
                    break;
                case ExtContentType.EXT_TYPE_BLOB_REF:
                    break;
                case ExtContentType.EXT_TYPE_BOOL:
                    a = attr.BoolVal();
                    break;
                case ExtContentType.EXT_TYPE_CLOB:
                    break;
                case ExtContentType.EXT_TYPE_CURRENCY:
                    break;
                case ExtContentType.EXT_TYPE_CURRENT:
                    break;
                case ExtContentType.EXT_TYPE_DATE:
                    break;
                case ExtContentType.EXT_TYPE_DOUBLE:
                    a = attr.DoubleVal();
                    break;
                case ExtContentType.EXT_TYPE_FREQUENCY:
                    break;
                case ExtContentType.EXT_TYPE_INT:
                    a = attr.IntVal();
                    break;
                case ExtContentType.EXT_TYPE_RESISTANCE:
                    break;
                case ExtContentType.EXT_TYPE_STRING:
                    a = attr.StrVal();
                    break;
                case ExtContentType.EXT_TYPE_ENUM:
                    a = attr.StrVal();
                    break;
                case ExtContentType.EXT_TYPE_TEMPERATURE:
                    break;
                case ExtContentType.EXT_TYPE_VOLTAGE:
                    break;
                case ExtContentType.LAST_EXT_CONT_TYPE:
                    break;
                default:
                    break;
            }
            return a;
        }

        public static object getAttr(string key, AvailableUnits unit)
        {
            object a = null;
            IAttribute attr = uda_Dic[key];
            switch (attr.ExtentionContentType())
            {
                case ExtContentType.EXT_TYPE_AREA:
                    a = attr.Area(unit);
                    break;
                case ExtContentType.EXT_TYPE_BLOB:
                    break;
                case ExtContentType.EXT_TYPE_BLOB_REF:
                    break;
                case ExtContentType.EXT_TYPE_BOOL:
                    a = attr.BoolVal();
                    break;
                case ExtContentType.EXT_TYPE_CLOB:
                    break;
                case ExtContentType.EXT_TYPE_CURRENCY:
                    break;
                case ExtContentType.EXT_TYPE_CURRENT:
                    break;
                case ExtContentType.EXT_TYPE_DATE:
                    break;
                case ExtContentType.EXT_TYPE_DOUBLE:
                    a = attr.DoubleVal();
                    break;
                case ExtContentType.EXT_TYPE_FREQUENCY:
                    break;
                case ExtContentType.EXT_TYPE_INT:
                    a = attr.IntVal();
                    break;
                case ExtContentType.EXT_TYPE_LENGTH:
                    a = attr.Length(unit);
                    break;
                case ExtContentType.EXT_TYPE_RESISTANCE:
                    break;
                case ExtContentType.EXT_TYPE_STRING:
                    a = attr.StrVal();
                    break;
                case ExtContentType.EXT_TYPE_TEMPERATURE:
                    break;
                case ExtContentType.EXT_TYPE_VOLTAGE:
                    break;
                case ExtContentType.EXT_TYPE_WEIGHT:
                    a = attr.Weight(unit);
                    break;
                case ExtContentType.LAST_EXT_CONT_TYPE:
                    break;
                default:
                    break;
            }
            return a;
        }

        public static object getAttr(string key, DevExpress.XtraEditors.ComboBoxEdit cmb)
        {
            object a = null;
            IAttribute attr = uda_Dic[key];
            switch (attr.ExtentionContentType())
            {
                case ExtContentType.EXT_TYPE_BLOB:
                    break;
                case ExtContentType.EXT_TYPE_BLOB_REF:
                    break;
                case ExtContentType.EXT_TYPE_BOOL:
                    a = attr.BoolVal();
                    break;
                case ExtContentType.EXT_TYPE_CLOB:
                    break;
                case ExtContentType.EXT_TYPE_CURRENCY:
                    break;
                case ExtContentType.EXT_TYPE_CURRENT:
                    break;
                case ExtContentType.EXT_TYPE_DATE:
                    break;
                case ExtContentType.EXT_TYPE_DOUBLE:
                    a = attr.DoubleVal();
                    break;
                case ExtContentType.EXT_TYPE_ENUM:
                    a = attr.StrVal();
                    Array enum_strings = attr.EnumStrings();
                    foreach (var e in enum_strings)
                    {
                        cmb.Properties.Items.Add(e);
                    }
                    break;
                case ExtContentType.EXT_TYPE_FREQUENCY:
                    break;
                case ExtContentType.EXT_TYPE_INT:
                    a = attr.IntVal();
                    break;
                case ExtContentType.EXT_TYPE_RESISTANCE:
                    break;
                case ExtContentType.EXT_TYPE_STRING:
                    a = attr.StrVal();
                    break;
                case ExtContentType.EXT_TYPE_TEMPERATURE:
                    break;
                case ExtContentType.EXT_TYPE_VOLTAGE:
                    break;
                case ExtContentType.LAST_EXT_CONT_TYPE:
                    break;
                default:
                    break;
            }
            return a;
        }

        public static int checkStatus(dynamic obj, string strObjType)
        {
            //System.Windows.Forms.MessageBox.Show(obj.ErrorStatus().ToString()); 
            if ((int)obj.ErrorStatus() != 0)
            {
                System.Windows.Forms.MessageBox.Show(obj.ErrorMessage() + "(" + obj.ErrorStatus() + ") in object type - " + strObjType + " in subsystem " + obj.ErrorSubSystem());
            }
            return (int)obj.ErrorStatus();
        }

        public static void setAttr(object newValue, string key, dynamic obj, AvailableUnits unit = AvailableUnits.MM)
        {
            IAttribute attr = uda_Dic[key];
            switch (attr.ExtentionContentType())
            {
                case ExtContentType.EXT_TYPE_AREA:
                    attModifier.SetArea(key, (double)newValue, unit);
                    break;
                case ExtContentType.EXT_TYPE_BLOB:
                    break;
                case ExtContentType.EXT_TYPE_BLOB_REF:
                    break;
                case ExtContentType.EXT_TYPE_BOOL:
                    attModifier.SetBoolean(key, (bool)newValue);
                    break;
                case ExtContentType.EXT_TYPE_CLOB:
                    break;
                case ExtContentType.EXT_TYPE_CURRENCY:
                    break;
                case ExtContentType.EXT_TYPE_CURRENT:
                    break;
                case ExtContentType.EXT_TYPE_DATE:
                    break;
                case ExtContentType.EXT_TYPE_DOUBLE:
                    attModifier.SetDouble(key, (double)newValue);
                    break;
                case ExtContentType.EXT_TYPE_ENUM:
                    attModifier.SetString(key, (string)newValue);
                    break;
                case ExtContentType.EXT_TYPE_FREQUENCY:
                    break;
                case ExtContentType.EXT_TYPE_INT:
                    attModifier.SetInteger(key, (int)newValue);
                    break;
                case ExtContentType.EXT_TYPE_LENGTH:
                    attModifier.SetLength(key, (double)newValue, unit);
                    break;
                case ExtContentType.EXT_TYPE_RESISTANCE:
                    break;
                case ExtContentType.EXT_TYPE_STRING:
                    attModifier.SetString(key, (string)newValue);
                    break;
                case ExtContentType.EXT_TYPE_TEMPERATURE:
                    break;
                case ExtContentType.EXT_TYPE_VOLTAGE:
                    break;
                case ExtContentType.EXT_TYPE_WEIGHT:
                    attModifier.SetWeight(key, (double)newValue, unit);
                    break;
                case ExtContentType.LAST_EXT_CONT_TYPE:
                    break;
                default:
                    break;
            }

        }

    }
}
